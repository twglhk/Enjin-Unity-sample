using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enjin.SDK.DataTypes;
using Enjin.SDK.GraphQL;
using Enjin.SDK.Template;
using Enjin.SDK.Utility;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

namespace Enjin.SDK.Core
{
    public class Enjin
    {
        #region Definitions

        private static EnjinIdentities _identities;
        private static EnjinCryptoItems _cryptoItems;
        private static EnjinUsers _users;
        private static EnjinRequests _requests;
        private static EnjinPlatform _platform;

        // Public properties
        public static string GraphQLURL { get; private set; }
        public static string APIURL { get; private set; }
        public static string AccessToken { get; set; }
        public static bool IsDebugLogActive { get; set; }
        public static bool IsLoggedIn { get; set; } = false;

        public static bool IsRequestValid(long code, string response)
        {
            return IsRequestResponseValid(code, response);
        }

        public static ErrorStatus ErrorReport { get; private set; }
        public static Dictionary<int, System.Action<RequestEvent>> RequestCallbacks { get; set; }
        public static Dictionary<string, List<System.Action<RequestEvent>>> EventListeners { get; set; }
        public static GraphQLTemplate UserTemplate { get; private set; }
        public static GraphQLTemplate PlatformTemplate { get; private set; }
        public static GraphQLTemplate IdentityTemplate { get; private set; }

        // Enums
        public static ResponseCodes ServerResponse { get; private set; }
        public static LoginState LoginState { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes primary objects for platform use
        /// </summary>
        private static void StartUp()
        {
            RequestCallbacks = new Dictionary<int, System.Action<RequestEvent>>();
            EventListeners = new Dictionary<string, List<System.Action<RequestEvent>>>();
            ErrorReport = new ErrorStatus();

            AccessToken = "";

            UserTemplate = new GraphQLTemplate("UserTemplate");
            PlatformTemplate = new GraphQLTemplate("PlatformTemplate");
            IdentityTemplate = new GraphQLTemplate("IdentityTemplate");

            _identities = new EnjinIdentities();
            _cryptoItems = new EnjinCryptoItems();
            _users = new EnjinUsers();
            _requests = new EnjinRequests();
            _platform = new EnjinPlatform();
        }

        /// <summary>
        /// Reports errors on server interaction if any
        /// </summary>
        /// <param name="code">Error code returned from server</param>
        /// <param name="response">Response description from server</param>
        private static bool IsRequestResponseValid(long code, string response)
        {
            if (response.Contains("errors"))
            {
                var errorGQL = JSON.Parse(response);
                ErrorReport.ErrorCode = errorGQL["errors"][0]["code"].AsInt;
                ErrorReport.ErrorMessage = errorGQL["errors"][0]["message"].Value;

                if (ErrorReport.ErrorCode != 0)
                    ServerResponse = (ResponseCodes) System.Convert.ToInt32(ErrorReport.ErrorCode);
                else
                    ServerResponse = ResponseCodes.INTERNAL;

                if (IsDebugLogActive)
                    Debug.Log("<color=red>[ERROR RESPONSE]</color> " + response);
            }
            else
                ServerResponse = (ResponseCodes) System.Convert.ToInt32(code);

            bool status = true;

            switch (ServerResponse)
            {
                case ResponseCodes.NOTFOUND:
                    Debug.Log("<color=red>[ERROR 404]</color> Request Not Found: " + response);
                    ResetErrorReport();
                    status = false;
                    break;

                case ResponseCodes.INVALID:
                    Debug.Log("<color=red>[ERROR 405]</color> Invalid Call to Serving URL: " + response);
                    ResetErrorReport();
                    status = false;
                    break;

                case ResponseCodes.DATACONFLICT:
                    Debug.Log("<color=red>[ERROR 409]</color> Object Already Exisits: " + response);
                    ResetErrorReport();
                    status = false;
                    break;

                case ResponseCodes.BADREQUEST:
                    Debug.Log("<color=red>[ERROR 400]</color> Bad Request: " + response);
                    ResetErrorReport();
                    status = false;
                    break;

                case ResponseCodes.UNAUTHORIZED:
                    Debug.Log("<color=red>[ERROR 401]</color> Unauthorized Request: " + response);
                    ResetErrorReport();
                    status = false;
                    break;

                case ResponseCodes.INTERNAL:
                    Debug.Log("<color=red>[ERROR 999]</color> Internal Request Bad: " + response);
                    ResetErrorReport();
                    status = false;
                    break;
            }

            return status;
        }

        /// <summary>
        /// Initializes the platform
        /// </summary>
        private static void InitializePlatform()
        {
            _platform.InitializePlatform();
            ServerResponse = ResponseCodes.INITIALIZED;
        }

        /// <summary>
        /// Sets all the url endpoints using the base API url as the prefix
        /// </summary>
        /// <param name="baseURL">Base API url prefix</param>
        private static void SetupAPI(string baseURL)
        {
            APIURL = baseURL;

            if (APIURL.EndsWith("/", System.StringComparison.Ordinal))
            {
                GraphQLURL = APIURL + "graphql";
            }
            else
            {
                GraphQLURL = APIURL + "/graphql";
            }
        }

        public static void StartPlatform(string baseApiUrl, int appId, string secret)
        {
            StartUp();
            SetupAPI(baseApiUrl);
            var result = _platform.AuthApp(appId, secret);

            if (!String.IsNullOrEmpty(result["accessToken"]))
            {
                LoginState = LoginState.VALID;
                AppID = _platform.ApplicationID = appId;
                AccessToken = result["accessToken"];
                InitializePlatform();
            }
        }

        public static void StartPlatformWithToken(string baseApiUrl, int appId, string accessToken)
        {
            StartUp();
            SetupAPI(baseApiUrl);

            if (!String.IsNullOrEmpty(accessToken))
            {
                LoginState = LoginState.VALID;
                AppID = _platform.ApplicationID = appId;
                AccessToken = accessToken;
                InitializePlatform();
            }
        }

        public static string AuthPlayer(string id)
        {
            return _platform.AuthPlayer(id)["accessToken"];
        }

        /// <summary>
        /// Cleans up the platform when exiting the applicaiton
        /// </summary>
        public static void CleanUpPlatform()
        {
            // Clean up any platform connections here
            LoginState = LoginState.NONE;
            IsLoggedIn = false;
            AccessToken = "";
            _platform.CleanUp();
        }

        /// <summary>
        /// Method to validate an Ethereum address
        /// </summary>
        /// <param name="address">Address to validate</param>
        /// <returns>True if address is valid, false otherwise</returns>
        public static bool ValidateAddress(string address)
        {
            Regex r = new Regex("^(0x){1}[0-9a-fA-F]{40}$");
            Regex r2 = new Regex("^(0x)?[0-9A-F]{40}$");

            if (r.IsMatch(address) || r2.IsMatch(address))
                return true;

            return false;
        }

        public static void ResetErrorReport()
        {
            ErrorReport = new ErrorStatus();
        }

        #endregion

        #region Identity Methods

        public static Wallet GetWalletBalances(string ethAddress)
        {
            return _identities.GetWalletBalances(ethAddress);
        }

        public static Wallet GetWalletBalancesForApp(string ethAddress, int appId)
        {
            return _identities.GetWalletBalancesForApp(ethAddress, appId);
        }

        public static bool UnLinkIdentity(int id)
        {
            return _identities.UnLink(id);
        }

        public static bool DeleteIdentity(string id)
        {
            return _identities.Delete(id);
        }

        public static Identity GetIdentity(int id)
        {
            return _identities.Get(id);
        }
        
        public static Identity CreateIdentity(Identity newIdentity)
        {
            return _identities.Create(newIdentity);
        }

        public static Identity UpdateIdentity(Identity identity)
        {
            return _identities.Update(identity);
        }

        #endregion

        #region Token Methods
        public static CryptoItem GetToken(string id)
        {
            return _cryptoItems.Get(id);
        }

        public static PaginationHelper<CryptoItem> GetTokens(int page, int limit)
        {
            return _cryptoItems.GetItems(page, limit);
        }

        #endregion

        #region Platform Methods

        public static int AppID
        {
            get { return _platform.ApplicationID; }
            set { _platform.ApplicationID = value; }
        }
        
        public static void ResetPusher()
        {
            _platform.PusherReconnect();
        }

        public static void SetAllowance(int identityId)
        {
            _platform.SetAllowance(identityId);
        }

        public static PlatformInfo GetPlatformInfo
        {
            get { return _platform.GetPlatform; }
        }

        public static App GetApp(int id)
        {
            return _platform.GetAppByID(id);
        }

        #endregion

        #region Request Methods
        
        public static string GetCryptoItemURI(string itemID, string itemIndex = "0", bool replaceTags = true)
        {
            return _requests.GetCryptoItemURI(itemID, itemIndex, replaceTags);
        }

        public static Request MintFungibleItem(int senderID, string[] addresses, string itemID, int value,
            bool async = false)
        {
            return _requests.MintFungibleItem(senderID, addresses, itemID, value, null, async);
        }

        public static Request MintFungibleItem(int senderID, string[] addresses, string itemID, int value,
            System.Action<RequestEvent> callback, bool async = false)
        {
            if (!async)
            {
                Request request = MintFungibleItem(senderID, addresses, itemID, value, async);
                RequestCallbacks.Add(request.id, callback);
                return request;
            }
            else
            {
                _requests.MintFungibleItem(senderID, addresses, itemID, value, (queryReturn) =>
                {
                    Request fullRequest = JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(queryReturn, 2));
                    RequestCallbacks.Add(fullRequest.id, callback);
                }, async);
                return null;
            }
        }

        public static Request MintNonFungibleItem(int senderID, string[] addresses, string itemID, bool async = false)
        {
            return _requests.MintNonFungibleItem(senderID, addresses, itemID, null, async);
        }

        public static Request MintNonFungibleItem(int senderID, string[] addresses, string itemID,
            System.Action<RequestEvent> callback, bool async = false)
        {
            if (!async)
            {
                Request request = MintNonFungibleItem(senderID, addresses, itemID, async);
                RequestCallbacks.Add(request.id, callback);
                return request;
            }
            else
            {
                _requests.MintNonFungibleItem(senderID, addresses, itemID, (queryReturn) =>
                {
                    Request fullRequest = JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(queryReturn, 2));
                    RequestCallbacks.Add(fullRequest.id, callback);
                }, async);
                return null;
            }
        }

        public static Request SetCryptoItemURI(int identityID, CryptoItem item, string URI,
            System.Action<RequestEvent> callback)
        {
            return _requests.SetCryptoItemURI(identityID, item, URI, callback);
        }

        public static Request GetRequest(int requestID)
        {
            return _requests.Get(requestID);
        }

        public static Request SendCryptoItemRequest(int identityID, string tokenID, int recipientID, int value,
            bool async = false)
        {
            return _requests.SendItem(identityID, tokenID, recipientID, value, null, async);
        }

        public static Request SendCryptoItemRequest(int identityID, string tokenID, int recipientID, int value,
            System.Action<RequestEvent> callback, bool async = false)
        {
            if (!async)
            {
                Request request = SendCryptoItemRequest(identityID, tokenID, recipientID, value, async);
                RequestCallbacks.Add(request.id, callback);
                return request;
            }
            else
            {
                _requests.SendItem(identityID, tokenID, recipientID, value, (queryReturn) =>
                {
                    Request fullRequest = JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(queryReturn, 2));
                    RequestCallbacks.Add(fullRequest.id, callback);
                }, async);
                return null;
            }
        }

        public static Request SendBatchCryptoItems(CryptoItemBatch items, int userID)
        {
            return _requests.SendItems(items, userID);
        }

        public static Request MeltTokens(int userIdentityID, string itemID, string index, int amount,
            bool async = false)
        {
            return _requests.MeltItem(userIdentityID, itemID, index, amount, null, async);
        }

        public static Request MeltTokens(int userIdentityID, string itemID, string index, int amount,
            System.Action<RequestEvent> callback, bool async = false)
        {
            if (!async)
            {
                Request request = MeltTokens(userIdentityID, itemID, index, amount, async);
                RequestCallbacks.Add(request.id, callback);
                return request;
            }
            else
            {
                _requests.MeltItem(userIdentityID, itemID, index, amount, (queryReturn) =>
                {
                    Request fullRequest = JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(queryReturn, 2));
                    RequestCallbacks.Add(fullRequest.id, callback);
                }, async);
                return null;
            }
        }

        public static Request MeltTokens(int userIdentityID, string itemID, int amount,
            System.Action<RequestEvent> callback, bool async = false)
        {
            if (!async)
            {
                Request request = MeltTokens(userIdentityID, itemID, "", amount, async);
                RequestCallbacks.Add(request.id, callback);
                return request;
            }
            else
            {
                _requests.MeltItem(userIdentityID, itemID, "", amount, (queryReturn) =>
                {
                    Request fullRequest = JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(queryReturn, 2));
                    RequestCallbacks.Add(fullRequest.id, callback);
                }, async);
                return null;
            }
        }

        public static Request UpdateCryptoItem(int identityID, CryptoItem item, CryptoItemFieldType fieldType,
            System.Action<RequestEvent> callback)
        {
            return _requests.UpdateCryptoItem(identityID, item, fieldType, callback);
        }

        public static Request CreateTradeRequest(int senderIdentityID, CryptoItem[] itemsFromSender,
            int[] amountsFromSender, string secondPartyAddress, CryptoItem[] itemsFromSecondParty,
            int[] amountsFromSecondParty)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, amountsFromSender,
                secondPartyAddress, null, itemsFromSecondParty, amountsFromSecondParty);
        }

        public static Request CreateTradeRequest(int senderIdentityID, CryptoItem[] itemsFromSender,
            int[] amountsFromSender, string secondPartyAddress, CryptoItem[] itemsFromSecondParty,
            int[] amountsFromSecondParty, System.Action<RequestEvent> callback)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, amountsFromSender,
                secondPartyAddress, null, itemsFromSecondParty, amountsFromSecondParty, callback);
        }

        public static Request CreateTradeRequest(int senderIdentityID, CryptoItem[] itemsFromSender,
            int[] amountsFromSender, int secondPartyIdentityID, CryptoItem[] itemsFromSecondParty,
            int[] amountsFromSecondParty)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, amountsFromSender, null,
                secondPartyIdentityID, itemsFromSecondParty, amountsFromSecondParty);
        }

        public static Request CreateTradeRequest(int senderIdentityID, CryptoItem[] itemsFromSender,
            int[] amountsFromSender, int secondPartyIdentityID, CryptoItem[] itemsFromSecondParty,
            int[] amountsFromSecondParty, System.Action<RequestEvent> callback)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, amountsFromSender, null,
                secondPartyIdentityID, itemsFromSecondParty, amountsFromSecondParty, callback);
        }

        public static Request CreateTradeRequest(int senderIdentityID, TokenValueInputData[] itemsFromSender,
            string secondPartyAddress, TokenValueInputData[] itemsFromSecondParty)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, secondPartyAddress, null,
                itemsFromSecondParty);
        }

        public static Request CreateTradeRequest(int senderIdentityID, TokenValueInputData[] itemsFromSender,
            string secondPartyAddress, TokenValueInputData[] itemsFromSecondParty, System.Action<RequestEvent> callback)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, secondPartyAddress, null,
                itemsFromSecondParty, callback);
        }

        public static Request CreateTradeRequest(int senderIdentityID, TokenValueInputData[] itemsFromSender,
            int secondPartyIdentityID, TokenValueInputData[] itemsFromSecondParty)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, null, secondPartyIdentityID,
                itemsFromSecondParty);
        }

        public static Request CreateTradeRequest(int senderIdentityID, TokenValueInputData[] itemsFromSender,
            int secondPartyIdentityID, TokenValueInputData[] itemsFromSecondParty, System.Action<RequestEvent> callback)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, null, secondPartyIdentityID,
                itemsFromSecondParty, callback);
        }

        public static Request CompleteTradeRequest(int senderIdentityID, string tradeID)
        {
            return _requests.CompleteTradeRequest(senderIdentityID, tradeID);
        }

        public static Request CompleteTradeRequest(int secondPartyID, string tradeID,
            System.Action<RequestEvent> callback)
        {
            return _requests.CompleteTradeRequest(secondPartyID, tradeID, callback);
        }

        #endregion

        #region User Methods

        public static User CreatePlayer(string name)
        {
            return _users.Create(name);
        }

        public static User GetUser(int id)
        {
            return _users.Get(id);
        }
        
        public static User GetUser(string name)
        {
            return _users.Get(name);
        }

        public static User GetCurrentUser()
        {
            return _users.GetCurrentUser();
        }

        #endregion

        #region Event Handler

        public static void BindEvent(string eventName, Action<RequestEvent> listener)
        {
            _platform.BindEvent(eventName, listener);
        }

        public static void ListenForLink(int identityID, Action<RequestEvent> listener)
        {
            _platform.ListenForLink(identityID, listener);
        }

        #endregion
    }
}