namespace Frends.Salesforce.PubSubPublish.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the publish operation completed successfully. True if successful, otherwise false.
    /// </summary>
    /// <example>true</example>
    /// <remarks>True if successful, otherwise false.</remarks>
    public bool Success { get; set; }

    /// <summary>
    /// The ID of the published message (if available). Base64-encoded replay ID.
    /// </summary>
    /// <example>"MTIzNDU2"</example>
    /// <remarks>Base64-encoded replay ID.</remarks>
    public string MessageId { get; set; }

    /// <summary>
    /// Error that occurred during task execution. Error details or null if none.
    /// </summary>
    /// <example>null</example>
    /// <remarks>Error details or null if none.</remarks>
    public Error Error { get; set; }
}
