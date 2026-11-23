using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// <returns>object { object Body, bool Success, object Error, string ErrorMessage, string Token }</returns>
    public static async Task<Result> CreateSObject(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, connection);

            string accessToken = await ConnectionHandler.GetAccessToken(connection, cancellationToken);
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

            var response = await client.ExecuteAsync(request, cancellationToken);

            var content = JsonConvert.DeserializeObject<dynamic>(response.Content);

            var result = new Result(
                content,
                response.IsSuccessful,
                response.ErrorException?.ToString(),
                response.ErrorMessage,
                accessToken);

            if (!response.IsSuccessful)
            {
                var responseMessage = response.ErrorMessage ??
                    $"Salesforce request failed with status code {(int)response.StatusCode}.";
                var responseException = response.ErrorException ?? new Exception(responseMessage);

                if (options.ThrowErrorOnFailure)
                {
                    throw responseException;
                }

                return responseException.Handle(options, result, responseMessage);
            }

            return result;
        }
        catch (JsonException e)
        {
            return e.Handle(options, additionalMessage: "Given input couldn't be parsed to json.");
        }
        catch (Exception e)
        {
            return e.Handle(options);
        }
    }
}
