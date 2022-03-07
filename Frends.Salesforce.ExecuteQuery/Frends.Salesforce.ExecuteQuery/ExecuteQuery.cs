using Newtonsoft.Json;
using RestSharp;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.Salesforce.ExecuteQuery
{
    /// <summary>
    /// Tasks class.
    /// </summary>
    public class Salesforce
    {
        /// <summary>
        /// Execute a query to salesforce
        /// [Documentation](https://github.com/FrendsPlatform/Frends.Salesforce/tree/main/Frends.Salesforce.ExecuteQuery)
        /// </summary>
        /// <returns></returns>
        public static async Task<Result> ExecuteQuery(
            [PropertyTab] Input input,
            [PropertyTab] Options options,
            CancellationToken cancellationToken
        )
        {
            var query = WebUtility.UrlEncode(input.Query);
            var client = new RestClient(input.Domain + "/services/data/v54.0/query/?q=" + query);
            var request = new RestRequest("/", Method.Get);
            string accessToken = "";

            if (options.AuthenticationMethod is AuthenticationMethod.AccessToken)
            {
                if (string.IsNullOrWhiteSpace(options.AccessToken)) throw new ArgumentException("Access token cannot be null when using Access Token authentication method");
                request.AddHeader("Authorization", "Bearer " + options.AccessToken);
            }

            if (options.AuthenticationMethod is AuthenticationMethod.OAuth2WithPassword)
            {
                accessToken = await GetAccessToken(options.ClientID, options.ClientSecret, options.Username, options.Password + options.SecurityToken, cancellationToken);
                request.AddHeader("Authorization", "Bearer " + accessToken);
            }

            var response = await client.ExecuteAsync(request, cancellationToken);

            if (options.AuthenticationMethod is AuthenticationMethod.OAuth2WithPassword && options.ReturnAccessToken)
                return new ResultWithToken { Body = JsonConvert.DeserializeObject<dynamic>(response.Content), RequestIsSuccessful = response.IsSuccessful, ErrorException = response.ErrorException, ErrorMessage = response.ErrorMessage, Token = accessToken };
            else
                return new Result { Body = JsonConvert.DeserializeObject<dynamic>(response.Content), RequestIsSuccessful = response.IsSuccessful, ErrorException = response.ErrorException, ErrorMessage = response.ErrorMessage };
        }

        #region HelperMethods

        /// <summary>
        /// Get OAuth2 access token.
        /// This method is public since it is used also in Unit tests.
        /// </summary>
        public static async Task<string> GetAccessToken(string clientId, string clientSecret, string username, string passwordWithSecurityToken, CancellationToken cancellationToken)
        {
            var authClient = new RestClient(@"https://login.salesforce.com/services/oauth2/token");
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
}
