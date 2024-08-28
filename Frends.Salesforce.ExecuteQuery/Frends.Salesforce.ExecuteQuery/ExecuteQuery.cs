using Frends.Salesforce.ExecuteQuery.Definitions;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Frends.Salesforce.ExecuteQuery.Tests")]
namespace Frends.Salesforce.ExecuteQuery;
/// <summary>
/// Tasks class.
/// </summary>
public class Salesforce
{
    /// <summary>
    /// Execute a query to Salesforce.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Salesforce.ExecuteQuery)
    /// </summary>
    /// <param name="input">Information to update the sobject.</param>
    /// <param name="options">Information about the salesforce destination.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Object { dynamic Body, bool RequestIsSuccessful, Exception ErrorException, string ErrorMessage, string Token }</returns>
    public static async Task<Result> ExecuteQuery(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(input.Domain)) throw new ArgumentNullException("Domain cannot be empty.");
        if (string.IsNullOrWhiteSpace(input.Query)) throw new ArgumentNullException("Query cannot be empty.");

        var query = WebUtility.UrlEncode(input.Query);
        var client = new RestClient(input.Domain + "/services/data/v54.0/query/?q=" + query);
        var request = new RestRequest("/", Method.Get);
        string accessToken = "";

        switch (options.AuthenticationMethod)
        {
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
            var response = await client.ExecuteAsync(request, cancellationToken);
            dynamic content = JsonConvert.DeserializeObject(response.Content);

            if (options.AuthenticationMethod is AuthenticationMethod.OAuth2WithPassword && options.ReturnAccessToken)
                return new Result(content, response.IsSuccessful, response.ErrorException, response.IsSuccessful ? string.Empty : content[0].Value<string>("message"), accessToken);
            else
                return new Result(content, response.IsSuccessful, response.ErrorException, response.IsSuccessful ? string.Empty : content[0].Value<string>("message"), string.Empty);
        }
        catch (ArgumentException)
        {
            throw new ArgumentException("Domain couldn't be found.");
        }
    }

    #region HelperMethods

    /// <summary>
    /// Get OAuth2 access token.
    /// This method is public since it is used also in Unit tests.
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
