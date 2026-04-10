using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.Salesforce.DeleteSObject.Attributes;

namespace Frends.Salesforce.DeleteSObject.Definitions;
/// <summary>
/// Input-class for DeleteSObject-task.
/// </summary>
public class Input
{
    /// <summary>
    /// SObject id.
    /// </summary>
    /// <example>abcdefghijkl123456789</example>
    [DisplayFormat(DataFormatString = "Text")]
    [NotEmptyString]
    public string SObjectId { get; set; }

    /// <summary>
    /// SObject type. Can be Account, Case, CustomObject, CustomSettings, CustomMetadata, etc...
    /// </summary>
    /// <example>Account</example>
    [DefaultValue("Account")]
    [DisplayFormat(DataFormatString = "Text")]
    [NotEmptyString]
    public string SObjectType { get; set; }
}
