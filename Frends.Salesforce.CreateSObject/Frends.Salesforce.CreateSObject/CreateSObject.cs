using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.CreateSObject.Definitions;
using Frends.Salesforce.CreateSObject.Helpers;
using Newtonsoft.Json;
using RestSharp;

namespace Frends.Salesforce.CreateSObject;

/// <summary>
/// Tasks class.
/// </summary>
public static class Salesforce
{
    /// <summary>
    /// Creates a sobject to Salesforce.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Salesforce.CreateSObject)
    /// </summary>
    /// <param name="input">Information to create the sobject.</param>
    /// <param name="connection">Information about the salesforce connection.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, object Body, string Token, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> CreateSObject(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        string accessToken = null;
        RestResponse response = null;

        try
        {
            ValidationHandler.Run(input, connection);

            accessToken = await ConnectionHandler.GetAccessToken(connection, cancellationToken);
            using var client =
                new RestClient(
                    $"{connection.InstanceUrl}/services/data/{connection.ApiVersion}/sobjects/{input.SObjectType}");
            var request = new RestRequest("/", Method.Post);
            request.AddHeader("Authorization", "Bearer " + accessToken);

            if (!ConnectionHandler.ShouldReturnToken(connection.AuthenticationMethod, connection.ReturnAccessToken))
                accessToken = string.Empty;

            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(input.SObjectAsJson);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(json);

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
        catch (Exception ex)
        {
            var result = ex.Handle(options);
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
