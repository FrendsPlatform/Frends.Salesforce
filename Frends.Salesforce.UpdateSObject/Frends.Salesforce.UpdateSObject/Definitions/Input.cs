using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Salesforce.UpdateSObject.Definitions;
/// <summary>
/// Input-class for CreateSObject-task.
/// </summary>
public class Input
{
    /// <summary>
    /// Salesforce Domain.
    /// /services/data/versionnumber/query will be added automatically, so the domain is enough.
    /// </summary>
    /// <example>https://example.my.salesforce.com</example>
    [DefaultValue(@"https://example.my.salesforce.com")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Domain { get; set; }

    /// <summary>
    /// The API version to use when making requests to Salesforce.
    /// If left empty, the default value is v61.0.
    /// </summary>
    [DefaultValue("v61.0")]
    public string ApiVersion { get; set; } = "v61.0";

    /// <summary>
    /// SObject structure as json.
    /// </summary>
    /// <example>{ "Name": "ExampleName" }</example>
    [DisplayFormat(DataFormatString = "Json")]
    public string SObjectAsJson { get; set; }

    /// <summary>
    /// SObject id.
    /// </summary>
    /// <example>abcdefghijkl123456789</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string SObjectId { get; set; }

    /// <summary>
    /// SObject type. Can be Account, Case, CustomObject, CustomSettings, CustomMetadata, etc...
    /// </summary>
    /// <example>Account</example>
    [DefaultValue("Account")]
    [DisplayFormat(DataFormatString = "Text")]
    public string SObjectType { get; set; }
}
