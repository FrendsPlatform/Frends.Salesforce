using Frends.Salesforce.UpdateSObject.Definitions;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.UpdateSObject.Helpers;

[assembly: InternalsVisibleTo("Frends.Salesforce.UpdateSObject.Tests")]

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
    /// <param name="cancellationToken"></param>
    /// <returns>Object { JObject Body, bool RequestIsSuccessful, Exception ErrorException, string ErrorMessage, string Token }</returns>
    public static async Task<Result> UpdateSObject(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        CancellationToken cancellationToken
    )
    {
        try
        {
            ValidationHandler.Run(input, connection);

            var client =
                new RestClient(
                    $"{connection.InstanceUrl}/services/data/{connection.ApiVersion}/sobjects/{input.SObjectType}/{input.SObjectId}");
            var request = new RestRequest("/", Method.Patch);
            var accessToken = await ConnectionHandler.GetAccessToken(connection, cancellationToken);
            request.AddHeader("Authorization", "Bearer " + accessToken);

            if (!ConnectionHandler.ShouldReturnToken(connection.AuthenticationMethod, connection.ReturnAccessToken))
                accessToken = string.Empty;


            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(input.SObjectAsJson);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(json);

            var response = await client.ExecuteAsync(request, cancellationToken);
            var content = JsonConvert.DeserializeObject<dynamic>(response.Content);

            if (connection.ThrowAnErrorIfNotFound && response.ErrorException != null && response.ErrorException.ToString()
                    .Equals(new HttpRequestException("Request failed with status code NotFound").ToString()))
                throw new HttpRequestException("Target couldn't be found with given id or type.");

            return new Result(content, response.IsSuccessful, response.ErrorException, response.ErrorMessage,
                accessToken);
        }
        catch (JsonReaderException e)
        {
            const string message = "Given input couldn't be parsed to json.";

            return ErrorHandler.Handle(e, message);
        }
        catch (RuntimeBinderException e)
        {
            const string message = "Given Salesforce information is invalid.";

            return ErrorHandler.Handle(e, message);
        }
        catch (Exception e)
        {
            return ErrorHandler.Handle(e);
        }
    }
}
