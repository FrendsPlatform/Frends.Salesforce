using Frends.Salesforce.CreateSObject.Definitions;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.CreateSObject.Helpers;

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
        CancellationToken cancellationToken
    )
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

            if (!response.IsSuccessful && options.ThrowErrorOnFailure)
            {
                var responseException = response.ErrorException ?? new Exception(response.ErrorMessage);
                return responseException.Handle(options);
            }

            if (!response.IsSuccessful && !string.IsNullOrEmpty(options.ErrorMessageOnFailure))
            {
                result = new Result(
                    content,
                    false,
                    response.ErrorException?.ToString(),
                    $"{options.ErrorMessageOnFailure}: {response.ErrorMessage}",
                    accessToken);
            }

            return result;
        }
        catch (JsonException e)
        {
            return e.Handle(options);
        }
        catch (Exception e)
        {
            return e.Handle(options);
        }
    }
}
