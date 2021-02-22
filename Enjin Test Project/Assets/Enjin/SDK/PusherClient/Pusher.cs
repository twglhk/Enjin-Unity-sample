using System;
using System.Collections.Generic;
using Enjin.SDK.PusherClient.Helper;

namespace Enjin.SDK.PusherClient
{
    /* TODO: Write tests
     * - Websocket disconnect
        - Connection lost, not cleanly closed
        - MustConnectOverSSL = 4000,
        - App does not exist
        - App disabled
        - Over connection limit
        - Path not found
        - Client over rate limie
        - Conditions for client event triggering
     */
    // TODO: NUGET Package
    // TODO: Ping & pong, are these handled by the Webscoket library out of the box?
    // TODO: Add assembly info file?
    // TODO: Implement connection fallback strategy

    // A delegate type for hooking up change notifications.
    public delegate void ConnectedEventHandler(object sender);

    public delegate void ConnectionStateChangedEventHandler(object sender, ConnectionState state);

    public class Pusher : EventEmitter
    {
        public static void Log(string message)
        {
            if (PusherSettings.Verbose)
                UnityEngine.Debug.Log("Pusher: " + message);
        }

        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning("Pusher: " + message);
        }

        const int PROTOCOL_NUMBER = 5;
        string _applicationKey = null;
        PusherOptions _options = null;

        public string Host = "ws.pusherapp.com";
        private Connection _connection = null;

        public event ConnectedEventHandler Connected;
        public event ConnectionStateChangedEventHandler ConnectionStateChanged;
        public Dictionary<string, Channel> Channels = new Dictionary<string, Channel>();

        #region Properties

        public string SocketID
        {
            get { return _connection.SocketID; }
        }

        public ConnectionState State
        {
            get { return _connection.State; }
        }

        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="Pusher" /> class.
        /// </summary>
        /// <param name="applicationKey">The application key.</param>
        /// <param name="options">The options.</param>
        public Pusher(string applicationKey, PusherOptions options = null)
        {
            _applicationKey = applicationKey;

            if (options == null)
                _options = new PusherOptions() {Encrypted = false};
            else
                _options = options;

            if (PusherSettings.HttpAuthUrl.Length > 0)
                _options.Authorizer = new HttpAuthorizer(PusherSettings.HttpAuthUrl);
        }

        #region Public Methods

        public void Connect()
        {
            // Check current connection state
            if (_connection != null)
            {
                switch (_connection.State)
                {
                    case ConnectionState.Connected:
                        LogWarning(
                            "Attempt to connect when connection is already in 'Connected' state. New attempt has been ignored.");
                        break;
                    case ConnectionState.Connecting:
                        LogWarning(
                            "Attempt to connect when connection is already in 'Connecting' state. New attempt has been ignored.");
                        break;
                    case ConnectionState.Failed:
                        LogWarning("Cannot attempt re-connection once in 'Failed' state");
                        throw new PusherException("Cannot attempt re-connection once in 'Failed' state",
                            ErrorCodes.ConnectionFailed);
                }
            }

            var scheme = "ws://";

            if (_options.Encrypted)
                scheme = "wss://";

            // TODO: Fallback to secure?

            string url = String.Format("{0}{1}/app/{2}?protocol={3}&client={4}&version={5}",
                scheme, _options.Host, _applicationKey, Pusher.PROTOCOL_NUMBER, PusherSettings.ClientName,
                PusherSettings.ClientVersion
            );

            Log("Connecting to url: '" + url + "'");
            _connection = new Connection(this, url);
            _connection.Connected += _connection_Connected;
            _connection.ConnectionStateChanged += _connection_ConnectionStateChanged;
            _connection.Connect();
        }

        public void Disconnect()
        {
            _connection.Disconnect();
        }

        public Channel Subscribe(string channelName)
        {
            if (_connection.State != ConnectionState.Connected)
                LogWarning("You must wait for Pusher to connect before you can subscribe to a channel");

            if (Channels.ContainsKey(channelName))
            {
                LogWarning("Channel '" + channelName +
                           "' is already subscribed to. Subscription event has been ignored.");
                return Channels[channelName];
            }

            // If private or presence channel, check that auth endpoint has been set
            var chanType = ChannelTypes.Public;

            if (channelName.ToLower().StartsWith("private-"))
                chanType = ChannelTypes.Private;
            else if (channelName.ToLower().StartsWith("presence-"))
                chanType = ChannelTypes.Presence;

            return SubscribeToChannel(chanType, channelName);
        }

        private Channel SubscribeToChannel(ChannelTypes type, string channelName)
        {
            switch (type)
            {
                case ChannelTypes.Public:
                    Channels.Add(channelName, new Channel(channelName, this));
                    break;
                case ChannelTypes.Private:
                    AuthEndpointCheck();
                    Channels.Add(channelName, new PrivateChannel(channelName, this));
                    break;
                case ChannelTypes.Presence:
                    AuthEndpointCheck();
                    Channels.Add(channelName, new PresenceChannel(channelName, this));
                    break;
            }

            if (type == ChannelTypes.Presence || type == ChannelTypes.Private)
            {
                Log("Calling auth for channel for: " + channelName);
                AuthorizeChannel(channelName);
            }
            else
            {
                // No need for auth details. Just send subscribe event
                _connection.Send(JsonHelper.Serialize(new Dictionary<string, object>()
                {
                    {"event", Constants.CHANNEL_SUBSCRIBE},
                    {
                        "data", new Dictionary<string, object>()
                        {
                            {"channel", channelName}
                        }
                    }
                }));
            }

            return Channels[channelName];
        }

        private void AuthorizeChannel(string channelName)
        {
            string authJson = _options.Authorizer.Authorize(channelName, _connection.SocketID);
            Log("Got replay from server auth: " + authJson);
            SendChannelAuthData(channelName, authJson);
        }

        private void SendChannelAuthData(string channelName, string jsonAuth)
        {
            // parse info from json data
            Dictionary<string, object> authDict = JsonHelper.Deserialize<Dictionary<string, object>>(jsonAuth);
            string authFromMessage = DataFactoryHelper.GetDictonaryValue(authDict, "auth", string.Empty);
            string channelDataFromMessage = DataFactoryHelper.GetDictonaryValue(authDict, "channel_data", string.Empty);

            _connection.Send(JsonHelper.Serialize(new Dictionary<string, object>()
            {
                {"event", Constants.CHANNEL_SUBSCRIBE},
                {
                    "data", new Dictionary<string, object>()
                    {
                        {"channel", channelName},
                        {"auth", authFromMessage},
                        {"channel_data", channelDataFromMessage}
                    }
                }
            }));
        }

        private void AuthEndpointCheck()
        {
            if (_options.Authorizer == null)
            {
                throw new PusherException(
                    "You must set a ChannelAuthorizer property to use private or presence channels",
                    ErrorCodes.ChannelAuthorizerNotSet);
            }
        }

        public void Send(string eventName, object data, string channel = null)
        {
            _connection.Send(JsonHelper.Serialize(new Dictionary<string, object>()
            {
                {"event", eventName},
                {"data", data},
                {"channel", channel}
            }));
        }

        #endregion

        #region Internal Methods

        internal void Trigger(string channelName, string eventName, object obj)
        {
            _connection.Send(JsonHelper.Serialize(new Dictionary<string, object>()
            {
                {"event", eventName},
                {"channel", channelName},
                {"data", obj}
            }));
        }

        internal void Unsubscribe(string channelName)
        {
            _connection.Send(JsonHelper.Serialize(new Dictionary<string, object>()
            {
                {"event", Constants.CHANNEL_UNSUBSCRIBE},
                {
                    "data", new Dictionary<string, object>()
                    {
                        {"channel", channelName}
                    }
                }
            }));
        }

        #endregion

        #region Connection Event Handlers

        private void _connection_ConnectionStateChanged(object sender, ConnectionState state)
        {
            if (ConnectionStateChanged != null)
                ConnectionStateChanged(sender, state);
        }

        void _connection_Connected(object sender)
        {
            if (this.Connected != null)
                this.Connected(sender);
        }

        #endregion
    }
}