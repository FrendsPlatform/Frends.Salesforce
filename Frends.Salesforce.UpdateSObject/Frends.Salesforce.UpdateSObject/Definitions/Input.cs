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
    /// /services/data/v52.0/query will be added automatically, so the domain is enough.
    /// </summary>
    /// <example>https://example.my.salesforce.com</example>
    [DefaultValue(@"https://example.my.salesforce.com")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Domain { get; set; }

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
