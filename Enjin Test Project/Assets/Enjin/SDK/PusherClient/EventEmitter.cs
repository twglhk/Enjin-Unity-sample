using System;
using System.Collections.Generic;
using Enjin.SDK.PusherClient.Helper;

namespace Enjin.SDK.PusherClient
{
    public class EventEmitter
    {
        private Dictionary<string, List<Action<object>>> _eventListeners =
            new Dictionary<string, List<Action<object>>>();

        private List<Action<string, object>> _generalListeners = new List<Action<string, object>>();

        public void Bind(string eventName, Action<object> listener)
        {
            if (_eventListeners.ContainsKey(eventName))
            {
                _eventListeners[eventName].Add(listener);
            }
            else
            {
                List<Action<object>> listeners = new List<Action<object>>();
                listeners.Add(listener);
                _eventListeners.Add(eventName, listeners);
            }
        }

        public void BindAll(Action<string, object> listener)
        {
            _generalListeners.Add(listener);
        }

        internal void EmitEvent(string eventName, string data)
        {
            var obj = JsonHelper.Deserialize<object>(data);

            // Emit to general listeners regardless of event type
            foreach (var action in _generalListeners)
            {
                action(eventName, obj);
            }

            if (_eventListeners.ContainsKey(eventName))
            {
                foreach (var action in _eventListeners[eventName])
                {
                    action(obj);
                }
            }
        }
    }
}