using System.Collections.Generic;
using Enjin.SDK.PusherClient.Helper;
using UnityEngine;

namespace Enjin.SDK.PusherClient
{
    internal class PusherEventData
    {
        public string eventName = string.Empty;
        public string channel = string.Empty;
        public string data = string.Empty;

        public static PusherEventData FromJson(string json)
        {
            PusherEventData data = new PusherEventData();
            Dictionary<string, object> dict = JsonHelper.Deserialize<Dictionary<string, object>>(json);
            if (dict != null)
            {
                data.eventName = DataFactoryHelper.GetDictonaryValue(dict, "event", string.Empty);
                data.data = DataFactoryHelper.GetDictonaryValue(dict, "data", string.Empty);
                data.channel = DataFactoryHelper.GetDictonaryValue(dict, "channel", string.Empty);
            }
            else
            {
                Debug.LogWarning("invalid pusher event data: '" + json + "'");
            }

            return data;
        }
    }
}