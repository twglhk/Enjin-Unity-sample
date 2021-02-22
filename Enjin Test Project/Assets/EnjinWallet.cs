using System.Collections.Generic;
using UnityEngine;
//using XLegacyEnjinSDK;
using System;
using System.Collections;
using Enjin.SDK.Core;
using Enjin.SDK.DataTypes;

namespace Enjin.SDK.Core
{
    public class EnjinWallet : MonoBehaviour
    {

        [SerializeField] string PLATFORM_URL;
        [SerializeField] string DEVELOPER_USERNAME;
        [SerializeField] int DEVELOPER_IDENTITY_ID;
        [SerializeField] int APP_ID;
        [SerializeField] string ACCESS_TOKEN;
        [SerializeField] string APP_SECRET;
        [SerializeField] string PLAYER_ID;

        string DEVELOPER_ADDRESS;
        string DEVELOPER_ACCESS_TOKEN;

        int PLAYER_IDENTITY_ID;
        string APP_LINK_CODE;
        string PLAYER_ADDRESS;

        private void Awake()
        {
            Enjin.IsDebugLogActive = true;
            Enjin.StartPlatform(PLATFORM_URL, APP_ID, APP_SECRET);
            StartCoroutine(LoginEnjin());
        }

        public IEnumerator LoginEnjin()
        {
            yield return null;

            while (Enjin.LoginState != LoginState.VALID)
            {
                Debug.Log("Connecting");
                yield return null;
            }

            Debug.Log("Connected");
            Debug.Log($"Access Token : {Enjin.AccessToken}");

            //Enjin.StartPlatformWithToken(PLATFORM_URL, APP_ID, ACCESS_TOKEN);

            User admin = Enjin.GetUser(DEVELOPER_USERNAME);
            DEVELOPER_ACCESS_TOKEN = Enjin.AccessToken;

            User player = Enjin.GetUser(PLAYER_ID);

            if (player == null)
            {
                Debug.Log("플레이어 Get 실패");
                Debug.Log("플레이어 생성");
                player = Enjin.CreatePlayer(PLAYER_ID);
            }

            Debug.Log($"Get Player Success : {player.name}");

            for (int i = 0; i < player.identities.Length; i++)
            {
                Debug.Log("PLAYER NAME : " + player.name);

                Identity identity = player.identities[i];

                Enjin.CreateIdentity(identity);
                if (identity.app.id == APP_ID)
                {
                    PLAYER_IDENTITY_ID = identity.id;
                    PLAYER_ADDRESS = identity.wallet.ethAddress;
                    APP_LINK_CODE = identity.linkingCode;
                    print("_IDENTITY_ID:: " + PLAYER_IDENTITY_ID);
                    print("_ADDRESS::" + PLAYER_ADDRESS);
                    print("_ADDRESS_LENGTH::" + PLAYER_ADDRESS.Length);
                    print("_LINKING_CODE::" + APP_LINK_CODE);

                }
            }


            // Enjin.CreatePlayer(PLAYER_EMAIL);
            print(Enjin.AuthPlayer(PLAYER_ID));


            yield return null;
        }

        IEnumerator MintItem(string itemName, int quantity)
        {
            string itemId = itemName;
            print(Enjin.GetCryptoItemURI(itemId));

            Enjin.MintFungibleItem(DEVELOPER_IDENTITY_ID, new string[] { PLAYER_ADDRESS }, itemId, quantity,
                (requestData) =>
                {

                    print("Item Minted::" + itemName);

                }, true);

            yield return null;
        }


        IEnumerator SendItem(string itemName, int quantiy)
        {
            string itemId = itemName;
            Enjin.SendCryptoItemRequest(PLAYER_IDENTITY_ID, itemName, DEVELOPER_IDENTITY_ID, quantiy, (requestData) =>
            {
                print("Item Sended::" + itemName);
            }, true);



            yield return null;
        }


        IEnumerator MeltItem(string itemName, int quantity)
        {
            string itemId = itemName;
            Enjin.MeltTokens(PLAYER_IDENTITY_ID, itemId, quantity, (requestData) =>
            {
                print("Item Melted::" + itemName);
            }, true);

            yield return null;
        }



        public void GetItem(string name)
        {
            print("Verifying transaction..");
            StartCoroutine(MintItem(name, 1));
        }

        public void ReturnItem(string name)
        {
            print("Verifying transaction..");
            StartCoroutine(MeltItem(name, 1));

        }

        public void SendItemTo(string name)
        {
            print("Verifying transaction..");
            StartCoroutine(SendItem(name, 1));
        }

    }
}