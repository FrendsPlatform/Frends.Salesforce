using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using Frends.Salesforce.ExecuteQuery.Definitions;
using Frends.Salesforce.ExecuteQuery.Helpers;
using Newtonsoft.Json;
using RestSharp;

[assembly: InternalsVisibleTo("Frends.Salesforce.ExecuteQuery.Tests")]

namespace Frends.Salesforce.ExecuteQuery;

/// <summary>
/// Tasks class.
/// </summary>
public static class Salesforce
{
    /// <summary>
    /// Execute a query to Salesforce.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Salesforce.ExecuteQuery)
    /// </summary>
    /// <param name="input">Information to update the sobject.</param>
    /// <param name="connection">Information about the salesforce destination.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, dynamic Body, string Token, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> ExecuteQuery(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        var accessToken = string.Empty;
        RestResponse response = null;

        try
        {
            ValidationHandler.Run(input, connection, options);

            var query = WebUtility.UrlEncode(input.Query);
            var client =
                new RestClient($"{connection.InstanceUrl}/services/data/{connection.ApiVersion}/query/?q={query}");
            var request = new RestRequest("/", Method.Get);
            accessToken = await ConnectionHandler.GetAccessToken(connection, cancellationToken);
            request.AddHeader("Authorization", "Bearer " + accessToken);

            if (!ConnectionHandler.ShouldReturnToken(connection.AuthenticationMethod, connection.ReturnAccessToken))
                accessToken = string.Empty;

            response = await client.ExecuteAsync(request, cancellationToken);
            var content = TryDeserialize(response.Content);

            if (!response.IsSuccessful)
            {
                if (cancellationToken.IsCancellationRequested && response.ErrorException is OperationCanceledException canceled)
                    throw canceled;

                var details = string.IsNullOrWhiteSpace(response.Content) ? string.Empty : $": {response.Content}";
                throw new HttpRequestException(
                    $"{response.ErrorException?.Message ?? response.ErrorMessage}{details}",
                    response.ErrorException);
            }

            return new Result
            {
                Success = true,
                Body = content,
                Token = accessToken,
                Error = null,
            };
        }
        catch (Exception e)
        {
            var result = e.Handle(options);
            result.Body = TryDeserialize(response?.Content);
            result.Token = accessToken;
            return result;
        }
    }

    private static object TryDeserialize(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;
        try
        {
            return JsonConvert.DeserializeObject<dynamic>(content);
        }
        catch (JsonException)
        {
            return content;
        }
    }
}
