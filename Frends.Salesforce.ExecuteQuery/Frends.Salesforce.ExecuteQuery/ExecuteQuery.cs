using Frends.Salesforce.ExecuteQuery.Definitions;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.ExecuteQuery.Helpers;

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
    /// <param name="cancellationToken"></param>
    /// <returns>Object { dynamic Body, bool RequestIsSuccessful, Exception ErrorException, string ErrorMessage, string Token }</returns>
    public static async Task<Result> ExecuteQuery(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        CancellationToken cancellationToken
    )
    {
        var accessToken = string.Empty;

        try
        {
            ValidationHandler.Run(input, connection);

            var query = WebUtility.UrlEncode(input.Query);
            var client =
                new RestClient($"{connection.InstanceUrl}/services/data/{connection.ApiVersion}/query/?q={query}");
            var request = new RestRequest("/", Method.Get);
            accessToken = await ConnectionHandler.GetAccessToken(connection, cancellationToken);
            request.AddHeader("Authorization", "Bearer " + accessToken);

            if (!ConnectionHandler.ShouldReturnToken(connection.AuthenticationMethod, connection.ReturnAccessToken))
                accessToken = string.Empty;

            var response = await client.ExecuteAsync(request, cancellationToken);
            dynamic content = JsonConvert.DeserializeObject(response.Content);

            return new Result(content, response.IsSuccessful, response.ErrorException,
                response.IsSuccessful ? string.Empty : content[0].Value<string>("message"), accessToken);
        }
        catch (Exception e)
        {
            return ErrorHandler.Handle(e, accessToken);
        }
    }
}
