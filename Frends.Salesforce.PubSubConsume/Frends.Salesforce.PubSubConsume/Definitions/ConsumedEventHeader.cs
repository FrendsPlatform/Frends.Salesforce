namespace Frends.Salesforce.PubSubConsume.Definitions;

/// <summary>
/// A Pub/Sub event header.
/// </summary>
public class ConsumedEventHeader
{
    /// <summary>
    /// Header key.
    /// </summary>
    /// <example>trace-parent</example>
    public string Key { get; set; }

    /// <summary>
    /// Header value encoded as Base64.
    /// </summary>
    /// <example>MDAtNGJmOTJhMzU3N2I0MzRkY2U5MjlkMGUyMGUwMDAwMDA=</example>
    public string ValueBase64 { get; set; }
}
