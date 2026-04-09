using Frends.Salesforce.DeleteSObject.Definitions;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.DeleteSObject.Helpers;

[assembly: InternalsVisibleTo("Frends.Salesforce.DeleteSObject.Tests")]

namespace Frends.Salesforce.DeleteSObject;

/// <summary>
/// Tasks class.
/// </summary>
public class Salesforce
{
    /// <summary>
    /// Deletes a sobject from Salesforce.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Salesforce.DeleteSObject)
    /// </summary>
    /// <param name="input">Information to delete the sobject.</param>
    /// <param name="options">Information about the salesforce destination.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Object { object Body, bool RequestIsSuccessful, Exception ErrorException, string ErrorMessage, string Token }</returns>
    public static async Task<Result> DeleteSObject(
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
            var request = new RestRequest("/", Method.Delete);
            var accessToken = await ConnectionHandler.GetAccessToken(connection, cancellationToken);
            request.AddHeader("Authorization", "Bearer " + accessToken);

            if (!ConnectionHandler.ShouldReturnToken(connection.AuthenticationMethod, connection.ReturnAccessToken))
                accessToken = string.Empty;

            var response = await client.ExecuteAsync(request, cancellationToken);
            var content = JsonConvert.DeserializeObject<dynamic>(response.Content);

            if (connection.ThrowAnErrorIfNotFound && response.ErrorException != null && response.ErrorException.ToString()
                    .Equals(new HttpRequestException("Request failed with status code NotFound").ToString()))
                throw new HttpRequestException("Target couldn't be found with given id.");

            return new Result(content, response.IsSuccessful, response.ErrorException, response.ErrorMessage,
                accessToken);
        }
        catch (Exception e)
        {
            return Helpers.ErrorHandler.Handle(e);
        }
    }
}
