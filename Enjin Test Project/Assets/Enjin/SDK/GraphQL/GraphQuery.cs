using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Enjin.SDK.GraphQL
{
    public class GraphQuery : MonoBehaviour
    {
        public static GraphQuery instance = null;

        public delegate void QueryComplete();

        public enum Status
        {
            Neutral,
            Loading,
            Complete,
            Error
        };

        public static Status queryStatus;
        public static string queryReturn;

        public class Query
        {
            public string query;
        }

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }

        public static Dictionary<string, string> variable = new Dictionary<string, string>();
        public static Dictionary<string, string[]> array = new Dictionary<string, string[]>();

        public static string GetEndPointData(string endpoint)
        {
            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Accept, "application/json");
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            client.Headers.Add(HttpRequestHeader.UserAgent, "Enjin-Unity-SDK-1.0.8");
            var response = client.DownloadString(endpoint);
            return response;
        }

        public static void POST(string details)
        {
            POST(details, "", false, null);
        }

        public static void POST(string details, string token)
        {
            POST(details, token, false, null);
        }

        public static void POST(string details, bool async)
        {
            POST(details, "", async, null);
        }

        public static void POST(string details, string token, bool async, System.Action<string> handler)
        {
            details = details.Trim('\r');
            details = QuerySorter(details);
            string jsonData = "";

            Query query = new Query {query = details};
            jsonData = JsonUtility.ToJson(query);

            if (Enjin.SDK.Core.Enjin.IsDebugLogActive)
            {
                if (!jsonData.Contains("password:") && !jsonData.Contains("accessTokens:"))
                    Debug.Log("<color=orange>[GRAPHQL QUERY]</color> " + jsonData);
            }

            UnityWebRequest request = UnityWebRequest.Post(Enjin.SDK.Core.Enjin.GraphQLURL, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData)) as UploadHandler;
            request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

            if (token != "login")
                request.SetRequestHeader("Authorization", "Bearer " + Enjin.SDK.Core.Enjin.AccessToken);

            request.downloadHandler = new DownloadHandlerBuffer();

            if (request.error != null)
            {
                Enjin.SDK.Core.Enjin.IsRequestValid(request.responseCode, request.downloadHandler.text);
            }
            else
            {
                if (async)
                {
                    instance.StartCoroutine(WaitForRequest(request, handler));
                    queryStatus = Status.Loading;
                }
                else if (!async)
                {
                    request.SendWebRequest();
                    while (!request.isDone)
                    {
                    }

                    if (Enjin.SDK.Core.Enjin.IsRequestValid(request.responseCode, request.downloadHandler.text))
                    {
                        queryStatus = Status.Complete;
                        queryReturn = request.downloadHandler.text;
                    }
                }
            }

            // NOTE: Turn this conversion on once methods are updated to support this structure
            //queryReturn = Regex.Replace(queryReturn, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1");

            if (Enjin.SDK.Core.Enjin.IsDebugLogActive && queryReturn != null)
                Debug.Log("<color=orange>[GRAPHQL RESULTS]</color> " + queryReturn.ToString());
        }

        static IEnumerator WaitForRequest(UnityWebRequest data, System.Action<string> handler)
        {
            yield return data.SendWebRequest();

            if (data.error != null)
            {
                Enjin.SDK.Core.Enjin.IsRequestValid(data.responseCode, data.downloadHandler.text);
                queryStatus = Status.Error;
            }
            else
            {
                if (Enjin.SDK.Core.Enjin.IsRequestValid(data.responseCode, data.downloadHandler.text))
                {
                    queryReturn = data.downloadHandler.text;
                    queryStatus = Status.Complete;

                    if (Enjin.SDK.Core.Enjin.IsDebugLogActive)
                        Debug.Log("<color=orange>[GRAPHQL RESULTS]</color> " + queryReturn);

                    handler(queryReturn);
                }
                else
                    queryReturn = "ERROR";
            }
        }

        public static string QuerySorter(string query)
        {
            string finalString;
            string[] splitString;
            string[] separators = {"$", "^"};

            splitString = query.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            finalString = splitString[0];

            for (int i = 1; i < splitString.Length; i++)
            {
                if (i % 2 == 0)
                {
                    finalString += splitString[i];
                }
                else
                {
                    if (!splitString[i].Contains("[]"))
                    {
                        finalString += variable[splitString[i]];
                    }
                    else
                    {
                        finalString += ArraySorter(splitString[i]);
                    }
                }
            }

            return finalString;
        }

        public static string ArraySorter(string theArray)
        {
            string[] anArray;
            string solution;

            anArray = array[theArray];
            solution = "[";

            for (int i = 0; i < anArray.Length; i++)
            {
                solution += "\"" + anArray[i];

                if (i < anArray.Length - 1)
                    solution += "\",";
                else
                    solution += "\"";
            }

            solution += "]";

            return solution;
        }
    }
}