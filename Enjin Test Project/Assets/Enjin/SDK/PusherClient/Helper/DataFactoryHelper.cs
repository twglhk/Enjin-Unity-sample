using System.Collections.Generic;

namespace Enjin.SDK.PusherClient.Helper
{
    public static class DataFactoryHelper
    {
        // utility method for loading string value from dictory, or default if not set
        public static string GetDictonaryValue(Dictionary<string, object> dictionary, string key, string defaultValue = "")
        {
            if (dictionary.ContainsKey(key))
            {
                if (dictionary[key] is string)
                {
                    return (string) dictionary[key];
                }
                else if (dictionary[key] != null)
                {
                    return dictionary[key].ToString();
                }
            }

            return defaultValue;
        }

        public static int GetDictonaryInt(Dictionary<string, object> dictionary, string key, int defaultValue)
        {
            string str = GetDictonaryValue(dictionary, key, "null");
            int result = defaultValue;
            if (str != "null")
            {
                if (!int.TryParse(str, out result))
                {
                    // Debug.LogWarning( "Failed to parse value as int, going with default.  Value was: '" + str + "', for key: '"+key+"'" );
                    result = defaultValue;
                }
            }

            return result;
        }

        public static double GetDictionaryDouble(Dictionary<string, object> dictionary, string key, double defaultValue)
        {
            if (!dictionary.ContainsKey(key))
            {
                return defaultValue;
            }
            else
            {
                object val = dictionary[key];
                if (val is double)
                {
                    return (double) val;
                }
                else
                {
                    return double.Parse(val.ToString());
                }
            }
        }

        public static bool GetDictonaryBool(Dictionary<string, object> dictionary, string key, bool defaultValue)
        {
            string str = GetDictonaryValue(dictionary, key, "null");
            return str != "null" ? bool.Parse(str) : defaultValue;
        }

        public static T EnumFromString<T>(string str)
        {
            System.Type enumType = typeof(T);
            T[] enumValues = (T[]) System.Enum.GetValues(enumType);
            foreach (T enumVal in enumValues)
            {
                if (enumVal.ToString() == str)
                    return enumVal;
            }

            throw new System.Exception(str + " is not a valid enum type for: " + enumType.ToString());
        }
    }
}