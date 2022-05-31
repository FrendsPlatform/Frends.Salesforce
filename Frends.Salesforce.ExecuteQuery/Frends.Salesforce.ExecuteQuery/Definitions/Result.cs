using Newtonsoft.Json.Linq;
using System;

namespace Frends.Salesforce.ExecuteQuery;

/// <summary>
/// Result-class for ExecuteQuery-task.
/// </summary>
public class Result
{
    /// <summary>
    /// Body of the response.
    /// </summary>
    /// <example>{"id": "abcdefghijkl123456789",  "success": true,  "errors": []}</example>
    public JObject Body { get; private set; }

    /// <summary>
    /// Was the request successful?
    /// </summary>
    /// <example>true</example>
    public bool RequestIsSuccessful { get; private set; }

    /// <summary>
    /// Exception that was thrown by the server.
    /// </summary>
    /// <example>System.Net.Http.HttpRequestException</example>
    public Exception ErrorException { get; private set; }

    /// <summary>
    /// Error message from the server.
    /// </summary>
    /// <example>System.Net.Http.HttpRequestException: Request failed with status code Unauthorized</example>
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// OAuth2 access token.
    /// </summary>
    /// <example>abcdefghjklmn123456789</example>
    public string Token { get; private set; }

    internal Result(JObject body, bool succesful, Exception error, string errormessage, string token)
    {
        Body = body;
        RequestIsSuccessful = succesful;
        ErrorException = error;
        ErrorMessage = errormessage;
        Token = token;
    }
}
