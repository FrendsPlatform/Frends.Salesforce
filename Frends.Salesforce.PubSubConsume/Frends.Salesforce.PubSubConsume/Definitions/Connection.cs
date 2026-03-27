using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Salesforce.PubSubConsume.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// Authentication method used to get a Salesforce access token.
    /// </summary>
    /// <example>UsernamePasswordOAuth</example>
    [DefaultValue(AuthenticationMethod.UsernamePasswordOAuth)]
    public AuthenticationMethod AuthenticationMethod { get; set; } = AuthenticationMethod.UsernamePasswordOAuth;

    /// <summary>
    /// Salesforce OAuth login URL. Used only with the UsernamePasswordOAuth authentication method.
    /// </summary>
    /// <example>https://login.salesforce.com</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("https://login.salesforce.com")]
    public string LoginUrl { get; set; } = "https://login.salesforce.com";

    /// <summary>
    /// Salesforce Pub/Sub API endpoint.
    /// </summary>
    /// <example>https://api.pubsub.salesforce.com:7443</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("https://api.pubsub.salesforce.com:7443")]
    public string PubSubApiUrl { get; set; } = "https://api.pubsub.salesforce.com:7443";

    /// <summary>
    /// Salesforce instance URL. Required when using the AccessToken authentication method.
    /// </summary>
    /// <example>https://mydomain.my.salesforce.com</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string InstanceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Salesforce tenant or org ID. Sent as the tenantId gRPC header.
    /// </summary>
    /// <example>00Dxx0000000001</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Reusable Salesforce access token. Required when using the AccessToken authentication method.
    /// </summary>
    /// <example>00Dxx0000000001!AQ8AQExampleToken</example>
    [PasswordPropertyText]
    [DefaultValue("")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// OAuth connected app client ID. Required when using the UsernamePasswordOAuth authentication method.
    /// </summary>
    /// <example>3MVG9d8..ExampleClientId</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth connected app client secret. Required when using the UsernamePasswordOAuth authentication method.
    /// </summary>
    /// <example>ExampleClientSecret</example>
    [PasswordPropertyText]
    [DefaultValue("")]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Salesforce username. Required when using the UsernamePasswordOAuth authentication method.
    /// </summary>
    /// <example>integration.user@example.com</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Salesforce password. Required when using the UsernamePasswordOAuth authentication method.
    /// </summary>
    /// <example>ExamplePassword</example>
    [PasswordPropertyText]
    [DefaultValue("")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Salesforce security token appended to the password for the username-password OAuth flow when required.
    /// </summary>
    /// <example>ExampleSecurityToken</example>
    [PasswordPropertyText]
    [DefaultValue("")]
    public string SecurityToken { get; set; } = string.Empty;
}
