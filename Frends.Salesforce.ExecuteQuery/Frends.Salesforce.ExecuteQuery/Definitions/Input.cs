using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Salesforce.ExecuteQuery;

/// <summary>
/// Input-class for ExecuteQuery-task.
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
    /// The API version to use when making requests to Salesforce.
    /// Default value is the latest version at this moment (v61.0).
    /// </summary>
    [DefaultValue("v61.0")]
    public string ApiVersion { get; set; } = "v61.0";

    /// <summary>
    /// Query which will be executed.
    /// </summary>
    /// <example>SELECT Name from Customer</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Query { get; set; }
}
