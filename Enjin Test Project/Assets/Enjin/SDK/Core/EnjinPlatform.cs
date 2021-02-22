using System;
using System.Collections.Generic;
using Enjin.SDK.GraphQL;
using Enjin.SDK.PusherClient;
using Enjin.SDK.PusherClient.Helper;
using Enjin.SDK.Utility;
using SimpleJSON;
using UnityEngine;

namespace Enjin.SDK.Core
{
    public enum ResponseCodes
    {
        INITIALIZED = 000,
        SUCCESS = 200,
        BADREQUEST = 400,
        UNAUTHORIZED = 401,
        NOTFOUND = 404,
        INVALID = 405,
        DATACONFLICT = 409,
        UNKNOWNERROR = 999,
        INTERNAL = 001
    }

    public enum LoginState
    {
        NONE,
        VALID,
        INVALIDUSERPASS,
        INVALIDTPURL,
        AUTO,
        UNAUTHORIZED
    }

    public enum SupplyModel
    {
        FIXED,
        SETTABLE,
        INFINITE,
        COLLAPSING,
        ANNUAL_VALUE,
        ANNUAL_PERCENTAGE
    }

    public enum SupplyModel2
    {
        FIXED,
        SETTABLE,
        INFINITE,
        COLLAPSING
    }

    public enum Transferable
    {
        PERMANENT,
        TEMPORARY,
        BOUND
    }

    public enum TransferType
    {
        NONE,
        PER_TRANSFER,
        PER_CRYPTO_ITEM,
        RATIO_CUT,
        RATIO_EXTRA
    } // TYPE_COUNT removed for V1, will be added back post V1

    public enum CryptoItemFieldType
    {
        NAME,
        TRANSFERABLE,
        TRANSFERFEE,
        MELTFEE,
        MAXMELTFEE,
        MAXTRANSFERFEE
    }

    public class EnjinPlatform
    {
        // Private variables & objects
        private int _appID; // Application ID
        private PlatformInfo _platformInfo; // Information about the platform

        // Pusher objects
        private Pusher _client; // Pucher client connector
        private Channel _channel; // Pusher channel connection
        private PusherOptions _options; // Pusher connection options

        // Public URL properties
        public int PlatformID
        {
            get { return System.Convert.ToInt32(_platformInfo.id); }
        }

        public int ApplicationID
        {
            get { return _appID; }
            set { _appID = value; }
        }

        public PlatformInfo GetPlatform
        {
            get
            {
                _platformInfo = GetPlatformInfo();
                return _platformInfo;
            }
        }

        public string TRData;

        /// <summary>
        /// Initializes platform
        /// </summary>
        public void InitializePlatform()
        {
            _platformInfo = GetPlatformInfo();

            PusherSettings.Verbose = false;
            _options = new PusherOptions
            {
                Cluster = _platformInfo.notifications.pusher.options.cluster,
                Encrypted = _platformInfo.notifications.pusher.options.encrypted == "true"
            };

            _client = new Pusher(_platformInfo.notifications.pusher.key, _options);
            _client.Connected += EventConnected;
            _client.ConnectionStateChanged += EventStateChange;
            _client.Connect();
        }

        /// <summary>
        /// Cleans up platform objects
        /// </summary>
        public void CleanUp()
        {
            _client.Disconnect();
        }

        /// <summary>
        /// Reconnects pusher on applicaiton change
        /// </summary>
        public void PusherReconnect()
        {
            _client.Disconnect();
            _client.Connect();
        }

        public JSONNode AuthApp(int appId, string secret)
        {
            var query = string.Format(Enjin.PlatformTemplate.GetQuery["AuthApp"], appId, secret);
            GraphQuery.POST(query, "login");
            var resultGql = JSON.Parse(GraphQuery.queryReturn);
            return resultGql["data"]["result"];
        }

        public JSONNode AuthPlayer(string id)
        {
            GraphQuery.POST(string.Format(global::Enjin.SDK.Core.Enjin.PlatformTemplate.GetQuery["AuthPlayer"], id));
            var resultGql = JSON.Parse(GraphQuery.queryReturn);
            return resultGql["data"]["result"];
        }
        
        /// <summary>
        /// Set the ENJ approval to max
        /// </summary>
        /// <param name="identityID">Identity of user to set max approval on</param>
        public void SetAllowance(int identityId)
        {
            GraphQuery.POST(string.Format(Enjin.PlatformTemplate.GetQuery["SetAllowance"], Enjin.AppID, identityId));
        }

        /// <summary>
        /// Geta an application's information by ID
        /// </summary>
        /// <param name="id">ID of application to get information for</param>
        /// <returns>Application information object</returns>
        public App GetAppByID(int id)
        {
            GraphQuery.POST(string.Format(Enjin.PlatformTemplate.GetQuery["GetAppByID"],
                id.ToString()));

            var resultGQL = JSON.Parse(GraphQuery.queryReturn);
            // TODO: Convert this to json parsing to datatype (Updates to read back in GraphQuery.cs)
            App appData = new App()
            {
                id = resultGQL["data"]["result"][0]["id"].AsInt,
                name = resultGQL["data"]["result"][0]["name"].Value,
                description = resultGQL["data"]["result"][0]["description"].Value,
                image = resultGQL["data"]["result"][0]["image"].Value
            };

            return appData;
        }

        /// <summary>
        /// Updates App information
        /// </summary>
        /// <param name="app">App to update information for</param>
        /// <returns>Updated App</returns>
        public App UpdateApp(App app)
        {
            string query;
            query =
                @"mutation updateApp{App:UpdateEnjinApp(name:""$appName^"",description:""$appDescription^"",image:""$appImageURL^""){id,name,description,image}}";
            GraphQuery.variable["appName"] = app.name;
            GraphQuery.variable["appDescription"] = app.description;
            GraphQuery.variable["appImageURL"] = app.image;
            GraphQuery.POST(query);

            return JsonUtility.FromJson<App>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));
        }

        /// <summary>
        /// Gets the platform information for intiializing platform
        /// </summary>
        /// <returns>PlatformInfo object containing platform info</returns>
        private PlatformInfo GetPlatformInfo()
        {
            GraphQuery.POST(Enjin.PlatformTemplate.GetQuery["GetPlatformInfo"], Enjin.AccessToken);

            return JsonUtility.FromJson<PlatformInfo>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
        }

        #region Pusher Methods

        /// <summary>
        /// Pusher connected event
        /// </summary>
        /// <param name="sender">Object connector for pusher</param>
        private void EventConnected(object sender)
        {
            if (global::Enjin.SDK.Core.Enjin.IsDebugLogActive)
                Debug.Log("<color=aqua>[PUSHER]</color> Client connected");

            _channel = _client.Subscribe("enjin.server." + _platformInfo.network
                                                         + "." + _platformInfo.id.ToString() +
                                                         "." + Enjin.AppID.ToString());
            _channel.BindAll(ChannelEvent);
        }

        /// <summary>
        /// Pusher event channel. Can be subscribed to for handling pusher events
        /// </summary>
        /// <param name="eventName">Event type</param>
        /// <param name="eventData">Data associated to event</param>
        private void ChannelEvent(string eventName, object eventData)
        {
            TRData = JsonHelper.Serialize(eventData);
            RequestEvent trackData = JsonUtility.FromJson<RequestEvent>(TRData);
            if (global::Enjin.SDK.Core.Enjin.IsDebugLogActive)
            {
                Debug.Log("<color=aqua>[PUSHER]</color> Event: " + trackData.event_type);
            }

            if (global::Enjin.SDK.Core.Enjin.IsDebugLogActive)
            {
                Debug.Log("<color=aqua>[PUSHER]</color> Event " + eventName + " recieved. Data: " + TRData);
            }

            // Temp fix for action duplication issue. Will replace with event manager integration
            // Execute any event handlers which are listening to this specific event.
            if (global::Enjin.SDK.Core.Enjin.EventListeners.ContainsKey(eventName))
            {
                for (int i = 0; i < global::Enjin.SDK.Core.Enjin.EventListeners[eventName].Count; i++)
                {
                    global::Enjin.SDK.Core.Enjin.EventListeners[eventName][i](trackData);
                }
            }

            // Notify any callback functions listening for this request that we've broadcasted.
            /*
            if (trackData.event_type.Equals("tx_broadcast"))
            {
                int requestId = trackData.data.id;
                if (Enjin.RequestCallbacks.ContainsKey(requestId))
                {
                    System.Action<RequestEvent> callback = Enjin.RequestCallbacks[requestId];
                    callback(trackData);
                }
            }
            */

            // Execute any callback function which is listening for this request.
            if (trackData.event_type.Equals("tx_executed"))
            {
                int requestId = trackData.data.transaction_id;
                if (global::Enjin.SDK.Core.Enjin.RequestCallbacks.ContainsKey(requestId))
                {
                    System.Action<RequestEvent> callback = global::Enjin.SDK.Core.Enjin.RequestCallbacks[requestId];
                    callback(trackData);
                    global::Enjin.SDK.Core.Enjin.RequestCallbacks.Remove(requestId);
                }
            }
        }

        /// <summary>
        /// Pusher state change. Reports any state changes from pusher
        /// </summary>
        /// <param name="sender">Object connector to track</param>
        /// <param name="state">State change of pusher connector</param>
        private void EventStateChange(object sender, ConnectionState state)
        {
            if (global::Enjin.SDK.Core.Enjin.IsDebugLogActive)
                Debug.Log("<color=aqua>[PUSHER]</color> Connection state changed to: " + state);
        }

        /// <summary>
        /// Bind a listener to fire each time some named event is received from pusher
        /// </summary>
        /// <param name="eventName">The string name of the event to track</param>
        /// <param name="listener">The listening action to fire with the responding event data</param>
        public void BindEvent(string eventName, System.Action<RequestEvent> listener)
        {
            bool hasListeners = global::Enjin.SDK.Core.Enjin.EventListeners.ContainsKey(eventName);
            if (hasListeners)
            {
                List<System.Action<RequestEvent>> listenerList = global::Enjin.SDK.Core.Enjin.EventListeners[eventName];
                listenerList.Add(listener);
                global::Enjin.SDK.Core.Enjin.EventListeners[eventName] = listenerList;
            }
            else
            {
                List<System.Action<RequestEvent>> listenerList = new List<System.Action<RequestEvent>>
                {
                    listener
                };
                global::Enjin.SDK.Core.Enjin.EventListeners[eventName] = listenerList;
            }
        }

        /// <summary>
        /// Bind a listener to fire when an event indicating that the given Identity ID has linked a wallet is received from pusher
        /// </summary>
        /// <param name="identityID">The integer ID of the Identity to listen for a linked wallet on</param>
        /// <param name="listener">The listening action to fire with the responding event data</param>
        internal void ListenForLink(int identityID, System.Action<RequestEvent> listener)
        {
            Channel channel = _client.Subscribe("enjin.server." + _platformInfo.network + "." +
                                                _platformInfo.id.ToString() + "." +
                                                global::Enjin.SDK.Core.Enjin.AppID.ToString() + "." +
                                                identityID);
            channel.BindAll((eventName, eventData) =>
            {
                string dataString = JsonHelper.Serialize(eventData);
                RequestEvent transactionData = JsonUtility.FromJson<RequestEvent>(dataString);
                if (global::Enjin.SDK.Core.Enjin.IsDebugLogActive)
                {
                    Debug.Log("<color=aqua>[PUSHER]</color> Event: " + transactionData.event_type);
                }

                if (global::Enjin.SDK.Core.Enjin.IsDebugLogActive)
                {
                    Debug.Log("<color=aqua>[PUSHER]</color> Event " + eventName + " recieved. Data: " + dataString);
                }

                // If we see that this client has updated their event, fire our awaiting callback.
                if (transactionData.event_type.Equals("identity_updated"))
                {
                    listener(transactionData);
                    channel.Unsubscribe();
                }
            });
        }

        #endregion
    }
}