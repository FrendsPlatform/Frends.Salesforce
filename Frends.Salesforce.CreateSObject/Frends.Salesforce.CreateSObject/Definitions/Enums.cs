using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frends.Salesforce.CreateSObject.Definitions
{
    /// <summary>
    /// Enums-class for CreateSObject-task.
    /// </summary>
    public class Enums
    {
        /// <summary>
        /// Authentication options to authenticate to Salesforce.
        /// </summary>
        public enum AuthenticationMethod
        {
            /// <summary>
            /// Authenticate with access token.
            /// </summary>
            AccessToken,
            /// <summary>
            /// Authenticate by providing required informations to fetch OAuth2 access token.
            /// </summary>
            OAuth2WithPassword
        }

        /// <summary>
        /// SObjectType options for Salesforce.
        /// </summary>
        public enum SObjectType
        {
            /// <summary>
            /// Account type for SObject.
            /// </summary>
            Account,
            /// <summary>
            /// Cases type for SObject.
            /// </summary>
            Cases,
            /// <summary>
            /// Custom object type for SObject.
            /// </summary>
            CustomObject,
            /// <summary>
            /// Custom settings type for SObject.
            /// </summary>
            CustomSettings,
            /// <summary>
            /// Custom metadata type for SObject.
            /// </summary>
            CustomMetadata
        }
    }
}
