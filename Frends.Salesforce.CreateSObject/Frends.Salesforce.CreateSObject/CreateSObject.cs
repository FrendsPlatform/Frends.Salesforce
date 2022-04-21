﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Text;
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
        /// Execute a query to salesforce
        /// </summary>
        /// <returns></returns>
        public static async Task<Result> CreateSObject(
            [PropertyTab] Input input,
            [PropertyTab] Options options,
            CancellationToken cancellationToken
        )
        {
            var query = WebUtility.UrlEncode(input.SObjectAsJson);
            var client = new RestClient(input.Domain + "/services/data/v54.0/" + input.SObjectType);
            var request = new RestRequest("/", Method.Get);
            string accessToken = "";

            if (options.AuthenticationMethod is AuthenticationMethod.AccessToken)
            {
                if (string.IsNullOrWhiteSpace(options.AccessToken)) throw new ArgumentException("Access token cannot be null when using Access Token authentication method");
                request.AddHeader("Authorization", "Bearer " + options.AccessToken);
            }

            if (options.AuthenticationMethod is AuthenticationMethod.OAuth2WithPassword)
            {
                accessToken = await GetAccessToken(options.AuthUrl, options.ClientID, options.ClientSecret, options.Username, options.Password + options.SecurityToken, cancellationToken);
                request.AddHeader("Authorization", "Bearer " + accessToken);
            }

            var response = await client.ExecuteAsync(request, cancellationToken);
            Console.WriteLine(response.Content);

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
