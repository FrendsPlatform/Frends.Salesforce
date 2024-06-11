namespace Frends.Salesforce.Authenticate.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Input class usually contains parameters that are required.
/// </summary>
public class Input
{
    /// <summary>
    /// Client ID of the Salesforce connected app.
    /// </summary>
    /// <example>3MVG9lKcPoNINVBIPJjdwSTvL9l7nP03cZ1op3spjl7xzp3L0LkJ6jZzF8a8jkz8gX8W8D9a90</example>
    public string ClientId { get; set; }

    /// <summary>
    /// Client Secret of the Salesforce connected app. Should be handled securely.
    /// </summary>
    /// <example>9D2B3AC1F1F8C26B99B1A7D</example>
    [PasswordPropertyText]
    public string ClientSecret { get; set; }

    /// <summary>
    /// Username of the Salesforce account.
    /// </summary>
    /// <example>user@example.com</example>
    public string Username { get; set; }

    /// <summary>
    /// Password of the Salesforce account.
    /// </summary>
    /// <example>password</example>
    [PasswordPropertyText]
    public string Password { get; set; }

    /// <summary>
    /// Security Token of the Salesforce account.
    /// </summary>
    /// <example>JHJHGFVT76HJ786G</example>
    [PasswordPropertyText]
    public string SecurityToken { get; set; }

    /// <summary>
    /// Login URL for the Salesforce authentication.
    /// </summary>
    /// <example>https://login.salesforce.com</example>
    public string LoginUrl { get; set; }
}