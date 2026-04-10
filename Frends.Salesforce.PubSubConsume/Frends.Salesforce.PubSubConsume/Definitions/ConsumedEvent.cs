using System.Collections.Generic;

namespace Frends.Salesforce.PubSubConsume.Definitions;

/// <summary>
/// A consumed Salesforce Pub/Sub event.
/// </summary>
public class ConsumedEvent
{
    /// <summary>
    /// Event identifier assigned by Salesforce or the publisher.
    /// </summary>
    /// <example>6d3b3ef5-9607-4e7b-91bb-1d8e280dc6f2</example>
    public string EventId { get; set; }

    /// <summary>
    /// Schema identifier for the event payload.
    /// </summary>
    /// <example>e4b5dc76b6f0f03f6aa9d68a71fbb9b9</example>
    public string SchemaId { get; set; }

    /// <summary>
    /// Replay ID encoded as Base64.
    /// </summary>
    /// <example>AAABBBCCC==</example>
    public string ReplayIdBase64 { get; set; }

    /// <summary>
    /// Event payload encoded as Base64.
    /// </summary>
    /// <example>AQIDBA==</example>
    public string PayloadBase64 { get; set; }

    /// <summary>
    /// Avro schema JSON for the payload when schema resolution is enabled.
    /// </summary>
    /// <example>{"type":"record","name":"MyEvent"}</example>
    public string SchemaJson { get; set; }

    /// <summary>
    /// Event headers.
    /// </summary>
    /// <example>object[] { object { string Key, string ValueBase64, string ValueUtf8Text } }</example>
    public List<ConsumedEventHeader> Headers { get; set; } = new();
}
