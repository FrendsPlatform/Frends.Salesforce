using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.UpdateSObject.Definitions;
using Frends.Salesforce.UpdateSObject.Helpers;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using RestSharp;

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
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, JObject Body, string Token, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> UpdateSObject(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
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
            {
                accessToken = string.Empty;
            }

            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(input.SObjectAsJson);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(json);

            var response = await client.ExecuteAsync(request, cancellationToken);
            var content = JsonConvert.DeserializeObject<dynamic>(response.Content);

            if (!response.IsSuccessful)
            {
                if (connection.ThrowAnErrorIfNotFound && response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new HttpRequestException("Target couldn't be found with given id or type.");
                }

                throw response.ErrorException ?? new HttpRequestException(response.ErrorMessage);
            }

            return new Result
            {
                Success = true,
                Body = content,
                Token = accessToken,
                Error = null,
            };
        }
        catch (JsonReaderException e)
        {
            return new Exception("Given input couldn't be parsed to json.", e).Handle(options);
        }
        catch (RuntimeBinderException e)
        {
            return new Exception("Given Salesforce information is invalid.", e).Handle(options);
        }
        catch (Exception e)
        {
            return e.Handle(options);
        }
    }
}
