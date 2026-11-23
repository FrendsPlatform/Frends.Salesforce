using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Frends.Salesforce.CreateSObject.Tests")]

namespace Frends.Salesforce.CreateSObject.Definitions;

/// <summary>
/// Result-class for CreateSObject-task.
/// </summary>
public class Result
{
    internal Result(object body, bool successful, string errorException, string errorMessage, string token)
    {
        Body = body;
        Success = successful;
        ErrorException = errorException;
        ErrorMessage = errorMessage;
        Token = token;
        Error = successful
            ? null
            : new Error { Message = errorMessage, AdditionalInfo = errorException };
    }

    /// <summary>
    /// Body of the response.
    /// </summary>
    /// <example>{"id": "abcdefghijkl123456789",  "success": true,  "errors": []}</example>
    public object Body { get; private set; }

    /// <summary>
    /// Indicates whether the request completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Error details, or null if the request succeeded.
    /// </summary>
    /// <example>null</example>
    public Error Error { get; private set; }

    /// <summary>
    /// Was the request successful?
    /// </summary>
    /// <example>true</example>
    public bool RequestIsSuccessful => Success;

    /// <summary>
    /// String representation of the exception returned by the HTTP client.
    /// </summary>
    /// <example>System.Net.Http.HttpRequestException: Request failed with status code Unauthorized</example>
    public string ErrorException { get; private set; }

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
}
