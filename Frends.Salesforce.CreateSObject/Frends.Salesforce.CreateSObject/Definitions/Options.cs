using System.ComponentModel;

namespace Frends.Salesforce.CreateSObject.Definitions;

/// <summary>
/// Additional task parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Whether to throw an error when the request fails.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message when the request fails.
    /// </summary>
    /// <example>Salesforce record creation failed.</example>
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
