namespace Frends.Salesforce.DeleteSObject.Definitions;

/// <summary>
/// Error that occurred during the task.
/// </summary>
public class Error
{
    /// <summary>
    /// Summary of the error.
    /// </summary>
    /// <example>Target couldn't be found with given id.</example>
    public string Message { get; set; }

    /// <summary>
    /// Additional information about the error.
    /// </summary>
    /// <example>System.Net.Http.HttpRequestException: Request failed with status code NotFound</example>
    public Exception AdditionalInfo { get; set; }
}
