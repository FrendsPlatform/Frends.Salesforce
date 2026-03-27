using System.Text.Json;
using Eventbus.V1;
using Frends.Salesforce.Toolkit.Definitions;
using Grpc.Core;
using Grpc.Net.Client;

namespace Frends.Salesforce.Toolkit.Handlers;

public static class ConnectionHandler
{
    private static GrpcChannel? channel;

    public static async Task<string> GetAccessToken(IConnection connection, CancellationToken cancellationToken)
    {
        return connection.AuthenticationMethod switch
        {
            AuthenticationMethod.AccessToken => connection.AccessToken,
            AuthenticationMethod.OAuth2WithPassword or AuthenticationMethod.OAuth2WithClientCredentials =>
                await GetOAuthResponse(connection, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(connection), "Invalid authentication method"),
        };
    }

    public static PubSub.PubSubClient GetPubSubClient(IPubSubConnection connection)
    {
        channel ??= GrpcChannel.ForAddress(connection.PubSubApiUrl, new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true,
            },
        });

        return new PubSub.PubSubClient(channel);
    }

    public static async Task ShutdownChannel()
    {
        if (channel is not null)
        {
            await channel.ShutdownAsync();
            channel.Dispose();
        }
    }

    public static Metadata GetMetadata(IPubSubConnection connection, string accessToken)
    {
        return new Metadata
        {
            {
                "accesstoken", accessToken
            },
            {
                "instanceurl", connection.InstanceUrl
            },
            {
                "tenantid", connection.TenantId
            },
        };
    }


    private static async Task<string> GetOAuthResponse(IConnection connection,
        CancellationToken cancellationToken)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(connection.AuthUrl),
        };

        var content = connection.AuthenticationMethod switch
        {
            AuthenticationMethod.OAuth2WithPassword => new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = connection.ClientId,
                ["client_secret"] = connection.ClientSecret,
                ["username"] = connection.Username,
                ["password"] = connection.Password + (connection.SecurityToken),
            }),
            AuthenticationMethod.OAuth2WithClientCredentials => new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = connection.ClientId,
                ["client_secret"] = connection.ClientSecret,
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(connection),
                "This method supports only OAuth2WithPassword and OAuth2WithClientCredentials"),
        };


        var request = new HttpRequestMessage(HttpMethod.Post, "/services/oauth2/token");
        request.Content = content;
        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Salesforce OAuth authentication failed with status code {response.StatusCode}: {responseContent}");
        }

        var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(responseContent);

        if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
        {
            throw new InvalidOperationException(
                "Salesforce OAuth authentication succeeded but the response did not contain an access token.");
        }

        return tokenResponse.AccessToken;
    }
}
