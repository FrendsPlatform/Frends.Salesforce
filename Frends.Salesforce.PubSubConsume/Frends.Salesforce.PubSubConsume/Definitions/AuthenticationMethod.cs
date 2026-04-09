namespace Frends.Salesforce.PubSubConsume.Definitions;

/// <summary>
/// Authentication options to authenticate to Salesforce.
/// </summary>
public enum AuthenticationMethod
{
    /// <summary>
    /// Authenticate with an access token.
    /// </summary>
    AccessToken = 0,

    /// <summary>
    /// Authenticate by providing required information to fetch OAuth2 access token.
    /// </summary>
    OAuth2WithPassword = 1,

    /// <summary>
    /// Authenticate using OAuth2 client credentials grant flow.
    /// </summary>
    OAuth2WithClientCredentials = 2,
}
