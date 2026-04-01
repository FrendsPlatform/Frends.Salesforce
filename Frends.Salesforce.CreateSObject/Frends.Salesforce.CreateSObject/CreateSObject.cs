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
    /// <param name="cancellationToken"></param>
    /// <returns>Object { object Body, bool RequestIsSuccessful, Exception ErrorException, string ErrorMessage, string Token }</returns>
    public static async Task<Result> CreateSObject(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        CancellationToken cancellationToken
    )
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

        try
        {
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(input.SObjectAsJson);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(json);

            var response = await client.ExecuteAsync(request, cancellationToken);

            var content = JsonConvert.DeserializeObject<dynamic>(response.Content);

            return new Result(content, response.IsSuccessful, response.ErrorException, response.ErrorMessage,
                accessToken);
        }
        catch (JsonException e)
        {
            return ErrorHandler.Handle(e, "Given input couldn't be parsed to json.");
        }
        catch (Exception e)
        {
            return ErrorHandler.Handle(e);
        }
    }
}
