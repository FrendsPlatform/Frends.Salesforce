using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.UpdateSObject.Definitions;
using Frends.Salesforce.UpdateSObject.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Frends.Salesforce.UpdateSObject;

/// <summary>
/// Tasks class.
/// </summary>
public static class Salesforce
{
    /// <summary>
    /// Updates a sobject from Salesforce.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Salesforce.UpdateSObject)
    /// </summary>
    /// <param name="input">Information to update the sobject.</param>
    /// <param name="connection">Information about the salesforce destination.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, JObject Body, string Token, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> UpdateSObject(
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

            var client =
                new RestClient(
                    $"{connection.InstanceUrl}/services/data/{connection.ApiVersion}/sobjects/{input.SObjectType}/{input.SObjectId}");
            var request = new RestRequest("/", Method.Patch);
            accessToken = await ConnectionHandler.GetAccessToken(connection, cancellationToken);
            request.AddHeader("Authorization", "Bearer " + accessToken);

            if (!ConnectionHandler.ShouldReturnToken(connection.AuthenticationMethod, connection.ReturnAccessToken))
            {
                accessToken = string.Empty;
            }

            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(input.SObjectAsJson);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(json);

            response = await client.ExecuteAsync(request, cancellationToken);
            var content = TryDeserialize(response.Content);

            if (!response.IsSuccessful)
            {
                if (cancellationToken.IsCancellationRequested && response.ErrorException is OperationCanceledException canceled)
                {
                    throw canceled;
                }

                if (connection.ThrowAnErrorIfNotFound && response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new HttpRequestException("Target couldn't be found with given id or type.");
                }

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

    private static JObject TryDeserialize(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<JObject>(content);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
