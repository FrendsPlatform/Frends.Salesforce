namespace Frends.Salesforce.PubSubConsume.Definitions;

/// <summary>
/// Supported Salesforce authentication methods.
/// </summary>
public enum AuthenticationMethod
{
    /// <summary>
    /// Use an already acquired Salesforce access token.
    /// </summary>
    AccessToken = 0,

    /// <summary>
    /// Acquire an access token with the OAuth username-password flow.
    /// </summary>
    UsernamePasswordOAuth = 1,
}
