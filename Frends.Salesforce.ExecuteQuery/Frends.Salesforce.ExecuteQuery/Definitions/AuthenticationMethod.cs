namespace Frends.Salesforce.ExecuteQuery.Definitions;

/// <summary>
/// Authentication options to authenticate to Salesforce.
/// </summary>
public enum AuthenticationMethod
{
    /// <summary>
    /// Authenticate with access token.
    /// </summary>
    AccessToken = 0,
    /// <summary>
    /// Authenticate by providing required informations to fetch OAuth2 access token.
    /// </summary>
    OAuth2WithPassword = 1,
    /// <summary>
    /// Authenticate by providing required informations to fetch OAuth2 access token client_credentials.
    /// </summary>
    OAuth2WithClientCredentials = 2
}