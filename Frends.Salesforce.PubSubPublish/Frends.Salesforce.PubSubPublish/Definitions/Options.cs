using System.ComponentModel;

namespace Frends.Salesforce.PubSubPublish.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Fetch Avro schema JSON for each distinct schema ID encountered in the response.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ResolveSchemas { get; set; } = true;

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Custom error message</example>
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
