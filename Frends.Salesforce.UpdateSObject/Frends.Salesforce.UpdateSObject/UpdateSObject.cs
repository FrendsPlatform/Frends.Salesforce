using Frends.Salesforce.UpdateSObject.Definitions;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Frends.Salesforce.UpdateSObject.Tests")]
namespace Frends.Salesforce.UpdateSObject;
/// <summary>
/// Tasks class.
/// </summary>
public class Salesforce
{
    /// <summary>
    /// Updates a sobject from Salesforce.
    /// [Documentation](https://tasks.frends.com/tasks#frends-tasks/Frends.Salesforce.UpdateSObject)
    /// </summary>
    /// <param name="input">Information to update the sobject.</param>
    /// <param name="options">Information about the salesforce destination.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Object { object Body, bool RequestIsSuccessful, Exception ErrorException, string ErrorMessage }</returns>
    public static async Task<Result> UpdateSObject(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(input.Domain)) throw new ArgumentNullException("Domain cannot be empty.");
        if (string.IsNullOrWhiteSpace(input.SObjectId)) throw new ArgumentNullException("Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(input.SObjectAsJson)) throw new ArgumentNullException("Json cannot be empty.");
        if (string.IsNullOrWhiteSpace(input.SObjectType)) throw new ArgumentNullException("Type cannot be empty.");

        var client = new RestClient(input.Domain + "/services/data/v54.0/sobjects/" + input.SObjectType + "/" + input.SObjectId);
        var request = new RestRequest("/", Method.Patch);
        string accessToken = "";

        switch (options.AuthenticationMethod) {
            case AuthenticationMethod.AccessToken:
                if (string.IsNullOrWhiteSpace(options.AccessToken)) throw new ArgumentException("Access token cannot be null when using Access Token authentication method");
                request.AddHeader("Authorization", "Bearer " + options.AccessToken);
                break;
            case AuthenticationMethod.OAuth2WithPassword:
                accessToken = await GetAccessToken(options.AuthUrl, options.ClientID, options.ClientSecret, options.Username, options.Password + options.SecurityToken, cancellationToken);
                request.AddHeader("Authorization", "Bearer " + accessToken);
                break;
        }

        try
        {
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(input.SObjectAsJson);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(json);

            var response = await client.ExecuteAsync(request, cancellationToken);
            var content = JsonConvert.DeserializeObject<dynamic>(response.Content);

            if (options.ThrowAnErrorIfNotFound && response.ErrorException.ToString().Equals(new HttpRequestException("Request failed with status code NotFound").ToString()))
                throw new HttpRequestException("Target couldn't be found with given id or type.");

            if (options.AuthenticationMethod is Definitions.AuthenticationMethod.OAuth2WithPassword && options.ReturnAccessToken)
                return new Result(content, response.IsSuccessful, response.ErrorException, response.ErrorMessage, accessToken);
            else
                return new Result(content, response.IsSuccessful, response.ErrorException, response.ErrorMessage, null);
        }
        catch (JsonReaderException)
        {
            throw new JsonReaderException("Given input couldn't be parsed to json.");
        }
        catch (ArgumentException)
        {
            throw new ArgumentException("Domain couldn't be found.");
        }
    }

    #region HelperMethods

    /// <summary>
    /// Get OAuth2 access token.
    /// </summary>
    internal static async Task<string> GetAccessToken(string url, string clientId, string clientSecret, string username, string passwordWithSecurityToken, CancellationToken cancellationToken)
    {
        var authClient = new RestClient(url);
        var authRequest = new RestRequest("", Method.Post);
        authRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        authRequest.AddParameter("grant_type", "password");
        authRequest.AddParameter("client_id", clientId);
        authRequest.AddParameter("client_secret", clientSecret);
        authRequest.AddParameter("username", username);
        authRequest.AddParameter("password", passwordWithSecurityToken);
        var authResponse = await authClient.ExecuteAsync(authRequest, cancellationToken);
        string accessToken = JsonConvert.DeserializeObject<dynamic>(authResponse.Content).access_token;
        return accessToken;
    }

    #endregion
}
