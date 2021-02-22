using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enjin.SDK.GraphQL;
using Enjin.SDK.Utility;
using SimpleJSON;
using UnityEngine;

namespace Enjin.SDK.Core
{
    public class EnjinCryptoItems
    {
        /// <summary>
        /// Gets all items in a pagination format
        /// </summary>
        /// <param name="page">Page to get</param>
        /// <param name="limit">Total items per page</param>
        /// <param name="identityID">Identity ID of user</param>
        /// <returns></returns>
        public PaginationHelper<CryptoItem> GetItems(int page, int limit)
        {
            string query = string.Empty;

            if (limit == 0)
                query =
                    @"query getAllItems{result:EnjinTokens(pagination:{page:$page^}){items{index,id,name,creator,totalSupply,reserve,circulatingSupply,supplyModel,meltValue,meltFeeRatio,meltFeeMaxRatio,transferable,transferFeeSettings{type,tokenId,value},nonFungible,icon,markedForDelete}cursor{total,hasPages,perPage,currentPage}}}";
            else
            {
                query =
                    @"query getAllItems{result:EnjinTokens(pagination:{limit:$limit^,page:$page^}){items{index,id,name,creator,totalSupply,reserve,circulatingSupply,supplyModel,meltValue,meltFeeRatio,meltFeeMaxRatio,transferable,transferFeeSettings{type,tokenId,value},nonFungible,icon,markedForDelete}cursor{total,hasPages,perPage,currentPage}}}";
                GraphQuery.variable["limit"] = limit.ToString();
            }

            GraphQuery.variable["page"] = page.ToString();
            GraphQuery.POST(query);

            return JsonUtility.FromJson<PaginationHelper<CryptoItem>>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
        }

        /// <summary>
        /// Gets a CryptoItem by it's ID
        /// </summary>
        /// <param name="id">ID of CryptoItem</param>
        /// <returns>CryptoItem of ID requested</returns>
        public CryptoItem Get(string id)
        {
            string query = string.Empty;

            try
            {
                query =
                    "query getCryptoItem{result:EnjinTokens(id:\"$id^\"){id,name,totalSupply,reserve,circulatingSupply,supplyModel,meltValue,meltFeeRatio,transferable,transferFeeSettings{type,tokenId,value},nonFungible,markedForDelete,itemURI}}";
                GraphQuery.variable["id"] = id.ToString();
                GraphQuery.POST(query);

                var tData = JsonUtility.FromJson<JSONArrayHelper<CryptoItem>>(
                    EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));

                return tData.result[0];
            }
            catch (Exception err)
            {
                Debug.LogWarning(err);
            }

            return null;
        }
    }
}