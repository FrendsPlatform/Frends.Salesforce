namespace Frends.Salesforce.UpdateSObject.Definitions;

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
