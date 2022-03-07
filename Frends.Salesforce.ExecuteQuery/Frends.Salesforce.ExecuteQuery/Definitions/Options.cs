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
        public AuthenticationMethod AuthenticationMethod { get; set; }

        /// <summary>
        /// OAuth2 access token.
        /// </summary>
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AccessToken)]
        [PasswordPropertyText]
        public string AccessToken { get; set; }

        /// <summary>
        /// Client ID to get OAuth2 token.
        /// </summary>
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
        public string ClientID { get; set; }

        /// <summary>
        /// Client secret to get OAuth2 access token.
        /// </summary>
        [PasswordPropertyText]
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Username of the user which will be used to fetch OAuth2 access token.
        /// </summary>
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
        public string Username { get; set; }

        /// <summary>
        /// Password for the user.
        /// </summary>
        [PasswordPropertyText]
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
        public string Password { get; set; }

        /// <summary>
        /// Security token for the user, which is required to add with the password by Salesforce.
        /// </summary>
        [PasswordPropertyText]
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
        public string SecurityToken { get; set; }

        /// <summary>
        /// Also return access token which is fetched during authentication?
        /// </summary>
        [DefaultValue(false)]
        [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
        public bool ReturnAccessToken { get; set; }
    }

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
}
