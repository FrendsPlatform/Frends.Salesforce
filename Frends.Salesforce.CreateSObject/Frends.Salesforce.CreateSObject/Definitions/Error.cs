using System;

namespace Frends.Salesforce.CreateSObject.Definitions;

/// <summary>
/// Error details returned by the task.
/// </summary>
public class Error
{
    /// <summary>
    /// Summary of the error.
    /// </summary>
    /// <example>Salesforce rejected the request.</example>
    public string Message { get; set; }

    /// <summary>
    /// Additional error details.
    /// </summary>
    /// <example>Request failed with status code Unauthorized.</example>
    public Exception AdditionalInfo { get; set; }
}
