using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enjin.SDK.GraphQL;
using Enjin.SDK.Utility;
using SimpleJSON;
using UnityEngine;

namespace Enjin.SDK.Core
{
    public class EnjinIdentities
    {

        /// <summary>
        /// Gets a specific identity
        /// </summary>
        /// <param name="id">ID of identity to get</param>
        /// <returns>Identity associated with passed id</returns>
        public Identity Get(int id)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["GetIdentity"], id.ToString()));

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return null;

            return JsonUtility
                .FromJson<JSONArrayHelper<Identity>>(EnjinHelpers.GetJSONString(
                    Regex.Replace(GraphQuery.queryReturn, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1"), 1)).result[0];
        }
        
        /// <summary>
        /// Creates a new identity
        /// </summary>
        /// <param name="newIdentity">New Identity to create</param>
        /// <returns>Created Identity</returns>
        public Identity Create(Identity newIdentity)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["CreateIdentity"],
                newIdentity.user.id.ToString(), newIdentity.wallet.ethAddress));

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return null;

            return JsonUtility.FromJson<Identity>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));
        }
        
        /// <summary>
        /// Updates an Identty
        /// </summary>
        /// <param name="identity">Identity to update</param>
        /// <returns>Updated Identity</returns>
        public Identity Update(Identity identity)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["UpdateIdentity"], identity.id.ToString(),
                identity.user.id.ToString(), identity.wallet.ethAddress));

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return null;

            return JsonUtility.FromJson<Identity>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));
        }

        /// <summary>
        /// Deletes an identity. if user attached to it, it's deleted as well
        /// </summary>
        /// <param name="id">Identitiy ID to delete</param>
        /// <returns>true/false on success</returns>
        public bool Delete(string id)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["DeleteIdentity"], id.ToString()));

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return false;

            return true;
        }

        /// <summary>
        /// Unlinks identity from wallet
        /// </summary>
        /// <param name="id">ID of identity to unlink</param>
        /// <returns>Updated identity</returns>
        public bool UnLink(int id)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["UnlinkIdentity"], id.ToString()));

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return false;

            return true;
        }

        public Wallet GetWalletBalances(string ethAddress)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["GetWalletBalances"], ethAddress));
            return JsonUtility.FromJson<Wallet>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
        }
        
        public Wallet GetWalletBalancesForApp(string ethAddress, int appId)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["GetWalletBalancesForApp"], ethAddress, appId.ToString()));
            return JsonUtility.FromJson<Wallet>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
        }
    }
}