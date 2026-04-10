using System.ComponentModel.DataAnnotations;
using Frends.Salesforce.ExecuteQuery.Attributes;

namespace Frends.Salesforce.ExecuteQuery.Definitions;

/// <summary>
/// Input-class for ExecuteQuery-task.
/// </summary>
public class Input
{
    /// <summary>
    /// Query which will be executed.
    /// </summary>
    /// <example>SELECT Name from Customer</example>
    [DisplayFormat(DataFormatString = "Text")]
    [NotEmptyString]
    public string Query { get; set; }
}
