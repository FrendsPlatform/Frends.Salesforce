using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.PubSubConsume.Definitions;

namespace Frends.Salesforce.PubSubConsume.Helpers;

internal static class SalesforceAuthentication
{
    internal static async Task<SalesforceSession> CreateSessionAsync(
        Connection connection,
        CancellationToken cancellationToken)
    {
        return connection.AuthenticationMethod switch
        {
            AuthenticationMethod.AccessToken => CreateAccessTokenSession(connection),
            AuthenticationMethod.UsernamePasswordOAuth => await CreateOAuthSessionAsync(connection, cancellationToken)
                .ConfigureAwait(false),
            _ => throw new InvalidOperationException(
                $"Unsupported authentication method '{connection.AuthenticationMethod}'."),
        };
    }

    internal static Uri NormalizeUri(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{parameterName}' must be provided.", parameterName);

        var normalized = value.Trim();
        if (!normalized.Contains("://", StringComparison.Ordinal))
            normalized = $"https://{normalized}";

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
            throw new ArgumentException($"'{parameterName}' is not a valid absolute URI.", parameterName);

        return uri;
    }

    private static SalesforceSession CreateAccessTokenSession(Connection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.AccessToken))
        {
            throw new ArgumentException(
                "'AccessToken' must be provided when using the AccessToken authentication method.",
                nameof(connection));
        }

        var instanceUri = NormalizeUri(connection.InstanceUrl, nameof(connection.InstanceUrl));

        return new SalesforceSession(
            connection.AccessToken.Trim(),
            instanceUri.GetLeftPart(UriPartial.Authority).TrimEnd('/'));
    }

    private static async Task<SalesforceSession> CreateOAuthSessionAsync(
        Connection connection,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connection.ClientId))
        {
            throw new ArgumentException(
                "'ClientId' must be provided when using the UsernamePasswordOAuth authentication method.",
                nameof(connection));
        }

        if (string.IsNullOrWhiteSpace(connection.ClientSecret))
        {
            throw new ArgumentException(
                "'ClientSecret' must be provided when using the UsernamePasswordOAuth authentication method.",
                nameof(connection));
        }

        if (string.IsNullOrWhiteSpace(connection.Username))
        {
            throw new ArgumentException(
                "'Username' must be provided when using the UsernamePasswordOAuth authentication method.",
                nameof(connection));
        }

        if (string.IsNullOrWhiteSpace(connection.Password))
        {
            throw new ArgumentException(
                "'Password' must be provided when using the UsernamePasswordOAuth authentication method.",
                nameof(connection));
        }

        using var httpClient = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildTokenEndpoint(connection.LoginUrl));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = connection.ClientId,
            ["client_secret"] = connection.ClientSecret,
            ["username"] = connection.Username,
            ["password"] = connection.Password + (connection.SecurityToken ?? string.Empty),
        });

        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Salesforce OAuth authentication failed with status code {(int)response.StatusCode}: {responseContent}");
        }

        var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException(
                "Salesforce OAuth authentication succeeded but the response did not contain an access token.");
        }

        if (string.IsNullOrWhiteSpace(tokenResponse.InstanceUrl))
        {
            throw new InvalidOperationException(
                "Salesforce OAuth authentication succeeded but the response did not contain an instance URL.");
        }

        var instanceUri = NormalizeUri(tokenResponse.InstanceUrl, nameof(tokenResponse.InstanceUrl));

        return new SalesforceSession(
            tokenResponse.AccessToken.Trim(),
            instanceUri.GetLeftPart(UriPartial.Authority).TrimEnd('/'));
    }

    private static Uri BuildTokenEndpoint(string loginUrl)
    {
        var baseUri = NormalizeUri(loginUrl, nameof(loginUrl));

        return new Uri(baseUri, "/services/oauth2/token");
    }

    private sealed class OAuthTokenResponse
    {
        /// <summary>
        /// OAuth access token.
        /// </summary>
        /// <example>00Dxx0000000001!AQ8AQExampleToken</example>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Salesforce instance URL.
        /// </summary>
        /// <example>https://mydomain.my.salesforce.com</example>
        [JsonPropertyName("instance_url")]
        public string InstanceUrl { get; set; }
    }
}
