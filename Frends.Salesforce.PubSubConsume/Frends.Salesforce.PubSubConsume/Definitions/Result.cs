using System.Collections.Generic;

namespace Frends.Salesforce.PubSubConsume.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Topic that was consumed.
    /// </summary>
    /// <example>/event/My_Event__e</example>
    public string TopicName { get; set; }

    /// <summary>
    /// Number of events returned in the Events collection.
    /// </summary>
    /// <example>3</example>
    public int EventCount { get; set; }

    /// <summary>
    /// Indicates whether the local wait timeout elapsed before the requested event count was reached.
    /// </summary>
    /// <example>false</example>
    public bool TimedOut { get; set; }

    /// <summary>
    /// Latest replay ID reported by Salesforce, encoded as Base64.
    /// </summary>
    /// <example>AAABBBCCC==</example>
    public string LatestReplayIdBase64 { get; set; }

    /// <summary>
    /// Number of events the server still considered pending in the latest response.
    /// </summary>
    /// <example>0</example>
    public int PendingNumRequested { get; set; }

    /// <summary>
    /// Events returned by the subscription.
    /// </summary>
    /// <example>object[] { object { string EventId, string SchemaId, string ReplayIdBase64, string PayloadBase64 } }</example>
    public List<ConsumedEvent> Events { get; set; } = new();

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
