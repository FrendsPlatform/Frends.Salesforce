using System.Text.Json.Serialization;

namespace Frends.Salesforce.Toolkit.Definitions;

public sealed class OAuthTokenResponse
{
    /// <summary>
    /// OAuth access token.
    /// </summary>
    /// <example>00Dxx0000000001!AQ8AQExampleToken</example>
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }
}
