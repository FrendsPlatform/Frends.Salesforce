using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.Common.Toolkit.Attributes;

namespace Frends.Salesforce.PubSubConsume.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Salesforce topic name to subscribe to e.g., /event/My_Event__e or /data/AccountChangeEvent.
    /// </summary>
    /// <example>/event/My_Event__e</example>
    [DefaultValue("")]
    [NotEmptyString]
    public string TopicName { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of events to receive before the task returns.
    /// </summary>
    /// <example>10</example>
    [DefaultValue(1)]
    [Range(1, int.MaxValue, ErrorMessage = "Number of events must be greater than 0")]
    public int NumberOfEvents { get; set; } = 1;

    /// <summary>
    /// Replay the starting point for the subscription.
    /// </summary>
    /// <example>Latest</example>
    [DefaultValue(ReplayPresetOption.Latest)]
    public ReplayPresetOption ReplayPreset { get; set; } = ReplayPresetOption.Latest;

    /// <summary>
    /// Base64 encoded replay ID to continue from when ReplayPreset is Custom.
    /// </summary>
    /// <example>AAABBBCCC==</example>
    [RequiredIf(nameof(ReplayPreset), ReplayPresetOption.Custom)]

    public string ReplayIdBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Maximum time to wait for events before returning. Use 0 to wait infinite or until the Frends cancellation token cancels the task.
    /// </summary>
    /// <example>30</example>
    [DefaultValue(30)]
    [Range(0, int.MaxValue, ErrorMessage = "Wait timeout can't be negative")]
    public int WaitTimeoutSeconds { get; set; } = 30;
}
