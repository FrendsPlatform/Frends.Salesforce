using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Frends.Salesforce.CreateSObject.Tests")]
namespace Frends.Salesforce.CreateSObject.Definitions;
/// <summary>
/// Result-class for CreateSObject-task.
/// </summary>
public class Result
{
    /// <summary>
    /// Body of the response.
    /// </summary>
    public object Body { get; private set; }

    /// <summary>
    /// Was the request successful?
    /// </summary>
    public bool RequestIsSuccessful { get; private set; }

    /// <summary>
    /// Exception that was thrown by the server.
    /// </summary>
    public Exception ErrorException { get; private set; }

    /// <summary>
    /// Error message from the server.
    /// </summary>
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// Used for testing.
    /// </summary>
    internal string RecordId { get; private set; }

    internal Result(object body, bool succesful, Exception error, string errormessage, string id)
    {
        this.Body = body;
        this.RequestIsSuccessful = succesful;
        this.ErrorException = error;
        this.ErrorMessage = errormessage;
        this.RecordId = id;
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
    public string Token { get; private set; }

    internal ResultWithToken(object body, bool succesful, Exception error, string errormessage, string token, string id) : base(body, succesful, error, errormessage, id)
    {
        this.Token = token;
    }
}
