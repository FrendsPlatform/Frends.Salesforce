using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Frends.Salesforce.CreateSObject.Tests")]
namespace Frends.Salesforce.DeleteSObject.Definitions;
/// <summary>
/// Result-class for CreateSObject-task.
/// </summary>
public class Result
{
    /// <summary>
    /// Body of the response.
    /// </summary>
    /// <example>{"id": "abcdefghijkl123456789",  "success": true,  "errors": []}</example>
    public object Body { get; private set; }

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

    internal Result(object body, bool succesful, Exception error, string errormessage)
    {
        Body = body;
        RequestIsSuccessful = succesful;
        ErrorException = error;
        ErrorMessage = errormessage;
    }
}

/// <summary>
/// Extended Result-class with access token.
/// </summary>
public class ResultWithToken : Result
{
    /// <summary>
    /// OAuth2 access token.
    /// </summary>
    /// <example>abcdefghjklmn123456789</example>
    public string Token { get; private set; }

    internal ResultWithToken(object body, bool succesful, Exception error, string errormessage, string token) : base(body, succesful, error, errormessage)
    {
        Token = token;
    }
}
