using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eventbus.V1;
using Frends.Salesforce.PubSubConsume.Definitions;
using Grpc.Core;
using Grpc.Net.Client;
using Newtonsoft.Json;
using RestSharp;

namespace Frends.Salesforce.PubSubConsume.Helpers;

internal static class ConnectionHandler
{
    private static GrpcChannel channel;

    internal static async Task<string> GetAccessToken(Connection connection, CancellationToken cancellationToken)
    {
        return connection.AuthenticationMethod switch
        {
            AuthenticationMethod.AccessToken => connection.AccessToken,
            AuthenticationMethod.OAuth2WithPassword or AuthenticationMethod.OAuth2WithClientCredentials =>
                await GetOAuthResponse(connection, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(connection), "Invalid authentication method"),
        };
    }

    internal static PubSub.PubSubClient GetPubSubClient(Connection connection)
    {
        if (channel == null || channel.State == ConnectivityState.Shutdown)
        {
            channel = GrpcChannel.ForAddress(connection.PubSubApiUrl, new GrpcChannelOptions
            {
                HttpHandler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,
                },
            });
        }

        return new PubSub.PubSubClient(channel);
    }

    internal static async Task ShutdownChannel()
    {
        if (channel is not null)
        {
            await channel.ShutdownAsync();
            channel.Dispose();
        }
    }

    internal static Metadata GetMetadata(Connection connection, string accessToken)
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

    private static async Task<string> GetOAuthResponse(Connection connection, CancellationToken cancellationToken)
    {
        using var authClient = new RestClient();
        RestRequest authRequest;

        switch (connection.AuthenticationMethod)
        {
            case AuthenticationMethod.OAuth2WithPassword:
                authRequest = new RestRequest(connection.AuthUrl + "/services/oauth2/token", Method.Post);
                authRequest.AddParameter("grant_type", "password");
                authRequest.AddParameter("client_id", connection.ClientId);
                authRequest.AddParameter("client_secret", connection.ClientSecret);
                authRequest.AddParameter("username", connection.Username);
                authRequest.AddParameter("password", connection.Password + connection.SecurityToken);

                break;
            case AuthenticationMethod.OAuth2WithClientCredentials:
                authRequest = new RestRequest(connection.InstanceUrl + "/services/oauth2/token", Method.Post);
                authRequest.AddParameter("grant_type", "client_credentials");
                authRequest.AddParameter("client_id", connection.ClientId);
                authRequest.AddParameter("client_secret", connection.ClientSecret);

                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(connection),
                    "This method supports only OAuth2WithPassword and OAuth2WithClientCredentials");
        }

        authRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");

        var authResponse = await authClient.ExecuteAsync(authRequest, cancellationToken);

        if (!authResponse.IsSuccessful)
        {
            throw new Exception(
                $"Failed to obtain access token. HTTP {(int)authResponse.StatusCode} ({authResponse.StatusCode}).");
        }

        dynamic responseContent = JsonConvert.DeserializeObject<dynamic>(authResponse.Content);
        string accessToken = responseContent?.access_token ?? throw new Exception("Access token not found in response");

        return accessToken;
    }
}
