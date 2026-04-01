using System.Text.Json.Serialization;

namespace Frends.Salesforce.PubSubConsume.Definitions;

internal class OAuthTokenResponse
{
    /// <summary>
    /// OAuth access token.
    /// </summary>
    /// <example>00Dxx0000000001!AQ8AQExampleToken</example>
    [JsonPropertyName("access_token")]
    internal required string AccessToken { get; set; }
}
