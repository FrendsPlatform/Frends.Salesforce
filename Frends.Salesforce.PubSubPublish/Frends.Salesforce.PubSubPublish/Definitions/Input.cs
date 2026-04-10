using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Salesforce.PubSubPublish.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// The topic or event name to publish to.
    /// </summary>
    /// <example>MyCustomEvent__e</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    [Required]
    public string TopicName { get; set; }

    /// <summary>
    /// The message payload to send in Json format correct with topic schema.
    /// </summary>
    /// <example>{ "field": "value" }</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    [Required]
    public string Payload { get; set; }

    /// <summary>
    /// Optional headers to include with the message.
    /// </summary>
    /// <example>object[] { object { string Key, string Value } }</example>
    public List<Header> Headers { get; set; }
}
