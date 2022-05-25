using Frends.Salesforce.ExecuteQuery.Definitions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Salesforce.ExecuteQuery
{
    /// <summary>
    /// Options-class for ExecuteQuery-task.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Authentication method.
        /// </summary>
        /// <example>AccessToken</example>
        public AuthenticationMethod AuthenticationMethod { get; set; }

        /// <summary>
        /// OAuth2 access token.
        /// </summary>
        /// <example>abcdefghijkl123456789</example>
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AccessToken)]
        [PasswordPropertyText]
        public string AccessToken { get; set; }

        /// <summary>
        /// URL to fetch OAuth2 token.
        /// </summary>
        /// <example>https://login.salesforce.com/services/oauth2/token</example>
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
        [DefaultValue(@"https://login.salesforce.com/services/oauth2/token")]
        [DisplayFormat(DataFormatString = "Text")]
        public string AuthUrl { get; set; }

        /// <summary>
        /// Client ID to get OAuth2 token.
        /// </summary>
        /// <example>abcdefghijkl123456789</example>
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
        [DisplayFormat(DataFormatString = "Text")]
        public string ClientID { get; set; }

        /// <summary>
        /// Client secret to get OAuth2 access token.
        /// </summary>
        /// <example>abcdefghijkl123456789</example>
        [PasswordPropertyText]
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Username of the user which will be used to fetch OAuth2 access token.
        /// </summary>
        /// <example>username123</example>
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
        [DisplayFormat(DataFormatString = "Text")]
        public string Username { get; set; }

        /// <summary>
        /// Password for the user.
        /// </summary>
        /// <example>password123</example>
        [PasswordPropertyText]
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
        public string Password { get; set; }

        /// <summary>
        /// Security token for the user, which is required to add with the password by Salesforce.
        /// </summary>
        /// <example>abcdefghijkl123456789</example>
        [PasswordPropertyText]
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
        public string SecurityToken { get; set; }

        /// <summary>
        /// Also return access token which is fetched during authentication?
        /// </summary>
        /// <example>true</example>
        [DefaultValue(false)]
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
        public bool ReturnAccessToken { get; set; }

        /// <summary>
        /// Set whether process will throw an error if targeted id can not be found from Salesforce.
        /// </summary>
        /// <example>true</example>
        [DefaultValue(false)]
        public bool ThrowAnErrorIfNotFound { get; set; }
    }
}
