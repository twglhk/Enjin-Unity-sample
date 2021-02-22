using UnityEngine;
using WebSocketSharp;

namespace Enjin.SDK.Utility
{
    public class EnjinHelpers
    {
        // Lookup Table for generating passwords
        private static string[] lookup = {"abcdefghijklmnopqrstuvwxyz", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "1234567890"};

        /// <summary>
        /// Converts int to a boolian string
        /// </summary>
        /// <param name="num">Int to convert</param>
        /// <returns>Boolian in string format ("True" / "False")</returns>
        public static string IntToBoolString(int num)
        {
            return (num == 1) ? "True" : "False";
        }

        /// <summary>
        /// JSON stripper to remove the first and/or second level data headers
        /// </summary>
        /// <param name="str">JSON string</param>
        /// <param name="levels">Number of header levels to strip ( 1 / 2)</param>
        /// <returns>Stripped Header level of JSON string</returns>
        public static string GetJSONString(string str, int levels)
        {
            return GetJSONString(str, ':', levels);
        }

        /// <summary>
        /// JSON stripper to remove the first and/or second level data headers
        /// </summary>
        /// <param name="str">JSON string</param>
        /// <returns>Stripped header 2 levels of JSON string</returns>
        public static string GetJSONString(string str)
        {
            return GetJSONString(str, ':', 2);
        }

        /// <summary>
        /// Generates pass code with 12 characters by default
        /// </summary>
        /// <returns>12 character pass code</returns>
        public static string GeneratePassCode()
        {
            return GeneratePassCode(12);
        }

        /// <summary>
        /// Gets the level of the JSON string based on lead string name
        /// </summary>
        /// <param name="str">String to parse</param>
        /// <param name="symbol">Symbol to parse from (default is ':')</param>
        /// <param name="levels">1 = non-array level, 2 = array level</param>
        /// <returns></returns>
        public static string GetJSONString(string str, char symbol, int levels)
        {
            if (str.IsNullOrEmpty())
                return str;
            
            int index = str.IndexOf(symbol) + 1;

            str = str.Substring(index, str.Length - 1 - index);

            if (levels >= 2)
            {
                index = str.IndexOf(symbol) + 1;
                if (str[index].Equals('['))
                {
                    str = str.Substring(index, str.Length - 2 - index);
                }
                else
                {
                    str = str.Substring(index, str.Length - 1 - index);
                }
            }

            if (levels == 3)
            {
                index = str.IndexOf(symbol) + 1;
                str = str.Substring(index, str.Length - 1 - index);
            }

            return str;
        }

        /// <summary>
        /// Generates a pass code with N number of characters
        /// </summary>
        /// <param name="length">Number of characters pass code should contain</param>
        /// <returns>N character pass code</returns>
        public static string GeneratePassCode(int length)
        {
            string word = "";
            int rnd;

            for (int i = 0; i < length; i++)
            {
                rnd = Random.Range(0, 3);
                word += lookup[rnd][Random.Range(0, lookup[rnd].Length)];
            }

            return word;
        }

        public static bool IsNullOrEmpty<T>(T[] array)
        {
            return array == null || array.Length < 1;
        }

        public static bool IsNullOrEmpty(string str)
        {
            return str == null || str.Length < 1;
        }
    }

    public class EnjinHelpers<T>
    {
        public static string ConvertToJSONArrayString(T[] str)
        {
            string result = "[";

            for (int i = 0; i < str.Length; i++)
            {
                if (typeof(T).ToString() == "System.String")
                    result += '"' + str[i].ToString() + '"';
                else
                    result += str[i].ToString();

                if (i != str.Length - 1)
                    result += ",";
            }

            result += "]";

            return result;
        }
    }
}