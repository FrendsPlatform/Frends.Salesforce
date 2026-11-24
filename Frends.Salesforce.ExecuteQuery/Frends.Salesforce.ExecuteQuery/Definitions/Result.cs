namespace Frends.Salesforce.ExecuteQuery.Definitions;

/// <summary>
/// Result-class for ExecuteQuery-task.
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
    /// <example>{"totalSize": 1, "done": true, "records": [{"Name": "Acme"}]}</example>
    public dynamic Body { get; set; }

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
