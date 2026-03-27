namespace Frends.Salesforce.PubSubConsume.Helpers;

/// <summary>
/// Authenticated Salesforce session details required by the Pub/Sub API.
/// </summary>
internal sealed class SalesforceSession
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SalesforceSession"/> class.
    /// </summary>
    /// <param name="accessToken">OAuth access token.</param>
    /// <param name="instanceUrl">Salesforce instance URL.</param>
    public SalesforceSession(string accessToken, string instanceUrl)
    {
        AccessToken = accessToken;
        InstanceUrl = instanceUrl;
    }

    /// <summary>
    /// OAuth access token.
    /// </summary>
    /// <example>00Dxx0000000001!AQ8AQExampleToken</example>
    public string AccessToken { get; }

    /// <summary>
    /// Salesforce instance URL.
    /// </summary>
    /// <example>https://mydomain.my.salesforce.com</example>
    public string InstanceUrl { get; }
}
