using Enjin.SDK.DataTypes;
using Enjin.SDK.GraphQL;
using Enjin.SDK.Utility;
using UnityEngine;

namespace Enjin.SDK.Core
{
    public class EnjinUsers
    {
        /// <summary>
        /// Creates a new User
        /// </summary>
        /// <param name="name">User's username</param>
        /// <returns>Created user object</returns>
        public User Create(string name)
        {
            GraphQuery.POST(string.Format(Enjin.UserTemplate.GetQuery["CreateUser"], name));

            if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
                return JsonUtility.FromJson<User>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));

            return null;
        }

        /// <summary>
        /// Gets a specified user from the current application
        /// </summary>
        /// <param name="userID">ID of user to get</param>
        /// <returns>Specified User</returns>
        public User Get(int id)
        {
            GraphQuery.POST(string.Format(Enjin.UserTemplate.GetQuery["GetUserForId"], id.ToString()));
            return JsonUtility.FromJson<User>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
        }
        
        /// <summary>
        /// Gets a specified user from the current application
        /// </summary>
        /// <param name="userID">ID of user to get</param>
        /// <returns>Specified User</returns>
        public User Get(string name)
        {
            GraphQuery.POST(string.Format(Enjin.UserTemplate.GetQuery["GetUserForName"], name));
            return JsonUtility.FromJson<User>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
        }

        public User GetCurrentUser()
        {
            GraphQuery.POST(Enjin.UserTemplate.GetQuery["GetCurrentUser"]);
            return JsonUtility.FromJson<User>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
        }
    }
}