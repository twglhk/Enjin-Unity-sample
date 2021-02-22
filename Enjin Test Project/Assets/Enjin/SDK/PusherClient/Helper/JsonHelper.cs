using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Enjin.SDK.PusherClient.Helper
{
    /*
 * Utilites + thin wrapper around MiniJSON.  If all JSON serializing / deserializing
 * goes through JsonHelper.Serialize / Deserialize then underlying JSON library can
 * be changed more easily
 */
    public static class JsonHelper
    {
        public static List<object> ToList(Vector2 ob)
        {
            List<object> list = new List<object>();
            list.Add(ob.x);
            list.Add(ob.y);
            return list;
        }

        public static List<object> ToList(Vector3 ob)
        {
            List<object> list = new List<object>();
            list.Add(ob.x);
            list.Add(ob.y);
            list.Add(ob.z);
            return list;
        }

        public static List<object> ToList(Vector4 ob)
        {
            List<object> list = new List<object>();
            list.Add(ob.x);
            list.Add(ob.y);
            list.Add(ob.z);
            list.Add(ob.w);
            return list;
        }

        public static List<object> ToList(Quaternion ob)
        {
            List<object> list = new List<object>();
            list.Add(ob.x);
            list.Add(ob.y);
            list.Add(ob.z);
            list.Add(ob.w);
            return list;
        }

        public static List<T> ToList<T>(T[] array)
        {
            List<T> list = new List<T>();
            foreach (T item in array)
            {
                list.Add(item);
            }

            return list;
        }

        public static float[] FloatArrayFromDoubleArray(double[] doubleArray)
        {
            float[] array = new float[doubleArray.Length];
            int i = 0;
            foreach (double dbl in doubleArray)
            {
                array[i] = (float) dbl;
                i++;
            }

            return array;
        }

        public static T[] ArrayFromList<T>(List<object> list)
        {
            T[] array = new T[list.Count];
            int i = 0;
            foreach (object ob in list)
            {
                array[i] = (T) ob;
                i++;
            }

            return array;
        }

        public static Vector2 Vector2FromList(List<object> list)
        {
            return new Vector3
            (
                (float) (double) list[0],
                (float) (double) list[1]
            );
        }

        public static Vector3 Vector3FromList(List<object> list)
        {
            return new Vector3
            (
                (float) (double) list[0],
                (float) (double) list[1],
                (float) (double) list[2]
            );
        }

        public static Vector4 Vector4FromList(List<object> list)
        {
            return new Vector4
            (
                (float) (double) list[0],
                (float) (double) list[1],
                (float) (double) list[2],
                (float) (double) list[3]
            );
        }

        public static Quaternion QuaternionFromList(List<object> list)
        {
            return new Quaternion
            (
                (float) (double) list[0],
                (float) (double) list[1],
                (float) (double) list[2],
                (float) (double) list[3]
            );
        }

        public static T[] EnumArrayFromList<T>(List<object> list)
        {
            T[] array = new T[list.Count];
            int i = 0;
            foreach (object ob in list)
            {
                array[i] = EnumFromObject<T>(ob);
                i++;
            }

            return array;
        }

        public static T EnumFromInteger<T>(int index)
        {
            if (!Enum.IsDefined(typeof(T), index))
                return default(T);

            return (T) Enum.ToObject(typeof(T), index);
        }

        public static T EnumFromObject<T>(object ob)
        {
            if (!Enum.IsDefined(typeof(T), (string) ob))
                return default(T);

            return (T) Enum.Parse(typeof(T), (string) ob);
        }

        public static List<T> ToTypedList<T>(object ob)
        {
            List<T> resultList = new List<T>();
            if (!(ob is List<object>))
            {
                Debug.LogWarning("Attempt to convert " + ob + " into List<object>");
                return resultList;
            }

            List<object> objList = (List<object>) ob;
            foreach (object o in objList)
            {
                if (o is T)
                    resultList.Add((T) o);
            }

            return resultList;
        }

        private const string INDENT_STRING = "    ";

        public static string FormatJson(string str)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            System.Linq.Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                        }

                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            System.Linq.Enumerable.Range(0, --indent)
                                .ForEach(item =>
                                    sb.Append(
                                        INDENT_STRING)); //NOTE: Including entire Linq library for a simple debug function, might not be a big deal but worth considering avoiding
                        }

                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            System.Linq.Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                        }

                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }

            return sb.ToString();
        }

        public static T Deserialize<T>(string json)
        {
            object obj = default(T);
            try
            {
                obj = Deserialize(json);
                if (obj is T)
                    return (T) obj;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Invalid json, exception while parsing: " + ex.Message);
            }

            return default(T);
        }

        /**
	 * MiniJSON versions of Serialize / Deserialize
	 */
        public static object Deserialize(string json)
        {
            object obj = null;
            try
            {
                obj = MiniJSON.Json.Deserialize(json);
            }
            catch (Exception)
            {
                obj = null;
            }

            return obj;
        }

        public static string Serialize(object obj)
        {
            return MiniJSON.Json.Serialize(obj);
        }
    }


    static class MiniJSONExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
            {
                action(i);
            }
        }
    }
}