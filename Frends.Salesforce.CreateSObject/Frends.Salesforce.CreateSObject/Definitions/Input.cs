using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Salesforce.CreateSObject.Definitions;
/// <summary>
/// Input-class for CreateSObject-task.
/// </summary>
public class Input
{
    /// <summary>
    /// SObject structure as json.
    /// </summary>
    /// <example>{ "Name": "ExampleName" }</example>
    [DisplayFormat(DataFormatString = "Json")]
    [Required]
    public string SObjectAsJson { get; set; }

    /// <summary>
    /// SObject type. Can be Account, Case, CustomObject, CustomSettings, CustomMetadata, etc...
    /// </summary>
    /// <example>Account</example>
    [DefaultValue("Account")]
    [Required]
    [DisplayFormat(DataFormatString = "Text")]
    public string SObjectType { get; set; } = "Account";
}
