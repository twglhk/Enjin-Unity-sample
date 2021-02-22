using System;
using System.Collections.Generic;
using Enjin.SDK.PusherClient.Helper;
using WebSocketSharp;

namespace Enjin.SDK.PusherClient
{
    internal class Connection
    {
        private WebSocket _websocket = null;
        private string _socketId = null;
        private string _url = null;
        private Pusher _pusher = null;
        private ConnectionState _state = ConnectionState.Initialized;
        private bool _allowReconnect = true;

        public event ConnectedEventHandler Connected;
        public event ConnectionStateChangedEventHandler ConnectionStateChanged;

        #region Properties

        internal string SocketID
        {
            get { return _socketId; }
        }

        internal ConnectionState State
        {
            get { return _state; }
        }

        #endregion

        public Connection(Pusher pusher, string url)
        {
            this._url = url;
            this._pusher = pusher;
        }

        #region Internal Methods

        internal void Connect()
        {
            // TODO: Handle and test disconnection / errors etc
            // TODO: Add 'connecting_in' event

            ChangeState(ConnectionState.Connecting);
            _allowReconnect = true;

            _websocket = new WebSocket(_url);
            _websocket.OnError += websocket_Error;
            _websocket.OnOpen += websocket_Opened;
            _websocket.OnClose += websocket_Closed;
            _websocket.OnMessage += websocket_MessageReceived;
            _websocket.EmitOnPing = true;
            _websocket.ConnectAsync();
        }

        internal void Disconnect()
        {
            _allowReconnect = false;
            _websocket.Close();
            ChangeState(ConnectionState.Disconnected);
        }

        internal void Send(string message)
        {
            Pusher.Log("Sending: " + message);
            _websocket.SendAsync(message, delegate(bool obj) { });
        }

        #endregion

        #region Private Methods

        private void ChangeState(ConnectionState state)
        {
            this._state = state;

            if (ConnectionStateChanged != null)
                ConnectionStateChanged(this, this._state);
        }

        private void websocket_MessageReceived(object sender, MessageEventArgs e)
        {
            Pusher.Log("Websocket message received: " + e.Data);

            if (e.IsPing)
            {
                Send("{\"event\": \"pusher:pong\"}");
                return;
            }

            PusherEventData message = PusherEventData.FromJson(e.Data);
            _pusher.EmitEvent(message.eventName, message.data);

            if (message.eventName.StartsWith("pusher"))
            {
                // Assume Pusher event
                switch (message.eventName)
                {
                    case Constants.ERROR:
                        ParseError(message.data);
                        break;

                    case Constants.CONNECTION_ESTABLISHED:
                        ParseConnectionEstablished(message.data);
                        break;

                    case Constants.CHANNEL_SUBSCRIPTION_SUCCEEDED:

                        if (_pusher.Channels.ContainsKey(message.channel))
                        {
                            var channel = _pusher.Channels[message.channel];
                            channel.SubscriptionSucceeded(message.data);
                        }

                        break;

                    case Constants.CHANNEL_SUBSCRIPTION_ERROR:

                        throw new PusherException("Error received on channel subscriptions: " + e.Data,
                            ErrorCodes.SubscriptionError);

                    case Constants.CHANNEL_MEMBER_ADDED:

                        // Assume channel event
                        if (_pusher.Channels.ContainsKey(message.channel))
                        {
                            var channel = _pusher.Channels[message.channel];

                            if (channel is PresenceChannel)
                            {
                                ((PresenceChannel) channel).AddMember(message.data);
                                break;
                            }
                        }

                        Pusher.LogWarning("Received a presence event on channel '" + message.channel +
                                          "', however there is no presence channel which matches.");
                        break;

                    case Constants.CHANNEL_MEMBER_REMOVED:

                        // Assume channel event
                        if (_pusher.Channels.ContainsKey(message.channel))
                        {
                            var channel = _pusher.Channels[message.channel];

                            if (channel is PresenceChannel)
                            {
                                ((PresenceChannel) channel).RemoveMember(message.data);
                                break;
                            }
                        }

                        Pusher.LogWarning("Received a presence event on channel '" + message.channel +
                                          "', however there is no presence channel which matches.");
                        break;
                }
            }
            else
            {
                // Assume channel event
                if (_pusher.Channels.ContainsKey(message.channel))
                    _pusher.Channels[message.channel].EmitEvent(message.eventName, message.data);
            }
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            Pusher.Log("Websocket opened OK.");
        }

        private void websocket_Closed(object sender, EventArgs e)
        {
            Pusher.Log("Websocket connection has been closed");

            ChangeState(ConnectionState.Disconnected);

            if (_allowReconnect)
                Connect();
        }

        private void websocket_Error(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            // TODO: What happens here? Do I need to re-connect, or do I just log the issue?
            Pusher.LogWarning("Websocket error: " + e.Message);
        }

        private void ParseConnectionEstablished(string data)
        {
            Dictionary<string, object> dict = JsonHelper.Deserialize<Dictionary<string, object>>(data);
            _socketId = DataFactoryHelper.GetDictonaryValue(dict, "socket_id", string.Empty);

            ChangeState(ConnectionState.Connected);

            if (Connected != null)
                Connected(this);
        }

        private void ParseError(string data)
        {
            Dictionary<string, object> dict = JsonHelper.Deserialize<Dictionary<string, object>>(data);
            string message = DataFactoryHelper.GetDictonaryValue(dict, "message", string.Empty);
            string errorCodeStr = DataFactoryHelper.GetDictonaryValue(dict, "code", ErrorCodes.Unkown.ToString());
            ErrorCodes error = DataFactoryHelper.EnumFromString<ErrorCodes>(errorCodeStr);

            throw new PusherException(message, error);
        }

        #endregion
    }
}