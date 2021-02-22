using System;
using Enjin.SDK.Core;

namespace Enjin.SDK.DataTypes
{
    /// <summary>
    /// User data structure that contains account info and links to user identity
    /// </summary>
    [Serializable]
    public class User
    {
        public int id; // Account ID
        public string name; // Username of account
        public DateData updatedAt; // Last update to account
        public DateData createAt; // Date account created
        public Identity[] identities; // Identity associated to user
        public IErrorHandle ErrorStatus; // Error handling object

        /// <summary>
        /// Constructor
        /// </summary>
        public User()
        {
            id = -1;
            name = string.Empty;
            updatedAt = new DateData();
            createAt = new DateData();
            ErrorStatus = new ErrorHandler();
            //identities = new List<Identity>();
        }
    }
}