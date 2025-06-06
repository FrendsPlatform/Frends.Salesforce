﻿using Frends.Salesforce.CreateSObject.Definitions;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.Salesforce.CreateSObject;
/// <summary>
/// Tasks class.
/// </summary>
public class Salesforce
{
    /// <summary>
    /// Creates a sobject to Salesforce.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Salesforce.CreateSObject)
    /// </summary>
    /// <param name="input">Information to create the sobject.</param>
    /// <param name="options">Information about the salesforce destination.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Object { object Body, bool RequestIsSuccessful, Exception ErrorException, string ErrorMessage, string Token }</returns>
    public static async Task<Result> CreateSObject(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(input.Domain)) throw new ArgumentNullException("Domain cannot be empty.");
        if (string.IsNullOrWhiteSpace(input.SObjectAsJson)) throw new ArgumentNullException("Json cannot be empty.");
        if (string.IsNullOrWhiteSpace(input.SObjectType)) throw new ArgumentNullException("Type cannot be empty.");

        var client = new RestClient($"{input.Domain}/services/data/{input.ApiVersion}/sobjects/{input.SObjectType}");
        var request = new RestRequest("/", Method.Post);
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
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(input.SObjectAsJson);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(json);

            var response = await client.ExecuteAsync(request, cancellationToken);
            var content = JsonConvert.DeserializeObject<dynamic>(response.Content);

            if (options.AuthenticationMethod is AuthenticationMethod.OAuth2WithPassword && options.ReturnAccessToken)
                return new Result(content, response.IsSuccessful, response.ErrorException, response.ErrorMessage, accessToken);
            else
                return new Result(content, response.IsSuccessful, response.ErrorException, response.ErrorMessage, string.Empty);
        }
        catch (JsonException)
        {
            throw new JsonException("Given input couldn't be parsed to json.");
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
