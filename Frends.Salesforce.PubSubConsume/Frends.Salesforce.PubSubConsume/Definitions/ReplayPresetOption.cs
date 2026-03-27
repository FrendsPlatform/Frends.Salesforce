namespace Frends.Salesforce.PubSubConsume.Definitions;

/// <summary>
/// Supported replay starting points for a subscription.
/// </summary>
public enum ReplayPresetOption
{
    /// <summary>
    /// Start from the tip of the stream.
    /// </summary>
    Latest = 0,

    /// <summary>
    /// Start from the oldest retained event in the stream.
    /// </summary>
    Earliest = 1,

    /// <summary>
    /// Start after a custom replay ID.
    /// </summary>
    Custom = 2,
}
