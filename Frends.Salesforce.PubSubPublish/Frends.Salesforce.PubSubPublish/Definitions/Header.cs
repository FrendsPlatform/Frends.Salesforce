namespace Frends.Salesforce.PubSubPublish.Definitions;

/// <summary>
/// Key-value pair for message headers.
/// </summary>
public class Header
{
    /// <summary>
    /// Header key.
    /// </summary>
    /// <example>X-Custom-Header</example>
    public string Key { get; set; }

    /// <summary>
    /// Header value.
    /// </summary>
    /// <example>CustomValue</example>
    public string Value { get; set; }
}
