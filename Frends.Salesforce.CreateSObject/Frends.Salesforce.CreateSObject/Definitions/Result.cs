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
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Body of the response.
    /// </summary>
    /// <example>{"id": "abcdefghijkl123456789",  "success": true,  "errors": []}</example>
    public object Body { get; set; }

    /// <summary>
    /// OAuth2 access token.
    /// </summary>
    /// <example>abcdefghjklmn123456789</example>
    public string Token { get; set; }

    /// <summary>
    /// Error details, or null if the request succeeded.
    /// </summary>
    /// <example>null</example>
    public Error Error { get; set; }
}
