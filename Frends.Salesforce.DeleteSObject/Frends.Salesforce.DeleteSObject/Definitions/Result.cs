namespace Frends.Salesforce.DeleteSObject.Definitions;

/// <summary>
/// Result-class for DeleteSObject-task.
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
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
