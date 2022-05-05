using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using static Frends.Salesforce.CreateSObject.Definitions.Enums;

namespace Frends.Salesforce.CreateSObject
{
    /// <summary>
    /// Tasks class.
    /// </summary>
    public class Salesforce
    {
        /// <summary>
        /// Execute a create call to salesforce
        /// </summary>
        /// <returns></returns>
        public static async Task<Result> CreateSObject(
            [PropertyTab] Input input,
            [PropertyTab] Options options,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrEmpty(input.Domain)) throw new ArgumentNullException("Domain cannot be empty.");
            else if (string.IsNullOrEmpty(input.SObjectAsJson)) throw new ArgumentNullException("Json cannot be empty.");
            else if (string.IsNullOrEmpty(input.SObjectType)) throw new ArgumentNullException("Type cannot be empty.");

            var client = new RestClient(input.Domain + "/services/data/v54.0/sobjects/" + input.SObjectType);
            var request = new RestRequest("/", Method.Post);
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

                if (options.AuthenticationMethod is AuthenticationMethod.OAuth2WithPassword && options.ReturnAccessToken)
                    return new ResultWithToken(content, response.IsSuccessful, response.ErrorException, response.ErrorMessage, accessToken, content.id.ToString());
                else
                    return new Result(content, response.IsSuccessful, response.ErrorException, response.ErrorMessage, content.id.ToString());
            }
            catch (JsonException)
            {
                throw new JsonException("Given input couldn't be parsed to json.");
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        #region HelperMethods

        /// <summary>
        /// Get OAuth2 access token.
        /// This method is public since it is used also in Unit tests.
        /// </summary>
        public static async Task<string> GetAccessToken(string url, string clientId, string clientSecret, string username, string passwordWithSecurityToken, CancellationToken cancellationToken)
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
}
