using System.ComponentModel;
using System.Runtime.CompilerServices;
using Frends.Salesforce.DeleteSObject.Definitions;
using Frends.Salesforce.DeleteSObject.Helpers;
using Newtonsoft.Json;
using RestSharp;

[assembly: InternalsVisibleTo("Frends.Salesforce.DeleteSObject.Tests")]

namespace Frends.Salesforce.DeleteSObject;

/// <summary>
/// Tasks class.
/// </summary>
public static class Salesforce
{
    /// <summary>
    /// Deletes a SObject from Salesforce.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Salesforce.DeleteSObject)
    /// </summary>
    /// <param name="input">Information to delete the sobject.</param>
    /// <param name="connection">Information about the Salesforce connection.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, object Body, string Token, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> DeleteSObject(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, connection, options);
            var client =
                new RestClient(
                    $"{connection.InstanceUrl}/services/data/{connection.ApiVersion}/sobjects/{input.SObjectType}/{input.SObjectId}");
            var request = new RestRequest("/", Method.Delete);
            var accessToken = await ConnectionHandler.GetAccessToken(connection, cancellationToken);
            request.AddHeader("Authorization", "Bearer " + accessToken);

            if (!ConnectionHandler.ShouldReturnToken(connection.AuthenticationMethod, connection.ReturnAccessToken))
                accessToken = string.Empty;

            var response = await client.ExecuteAsync(request, cancellationToken);

            if (connection.ThrowAnErrorIfNotFound && response.ErrorException != null && response.ErrorException.ToString()
                    .Equals(new HttpRequestException("Request failed with status code NotFound").ToString()))
                throw new HttpRequestException("Target couldn't be found with given id.");

            if (!response.IsSuccessful)
            {
                if (cancellationToken.IsCancellationRequested && response.ErrorException is OperationCanceledException canceled)
                    throw canceled;

                var details = string.IsNullOrWhiteSpace(response.Content) ? string.Empty : $": {response.Content}";
                var ex = new HttpRequestException(
                    $"{response.ErrorException?.Message ?? response.ErrorMessage}{details}",
                    response.ErrorException);

                if (options.ThrowErrorOnFailure)
                    throw ex;

                var result = ex.Handle(options);
                result.Body = TryDeserialize(response.Content);
                result.Token = accessToken;
                return result;
            }

            return new Result
            {
                Success = true,
                Body = TryDeserialize(response.Content),
                Token = accessToken,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
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
