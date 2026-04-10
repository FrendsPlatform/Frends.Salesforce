using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.Salesforce.PubSubConsume.Attributes;

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
    [DefaultValue(AuthenticationMethod.OAuth2WithPassword)]
    public AuthenticationMethod AuthenticationMethod { get; set; } = AuthenticationMethod.OAuth2WithPassword;

    /// <summary>
    /// Salesforce Pub/Sub API endpoint.
    /// </summary>
    /// <example>https://api.pubsub.salesforce.com:7443</example>
    [NotEmptyString]
    public string PubSubApiUrl { get; set; } = string.Empty;

    /// <summary>
    /// Salesforce instance URL. Required when using the AccessToken authentication method.
    /// </summary>
    /// <example>https://mydomain.my.salesforce.com</example>
    [NotEmptyString]
    public string InstanceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Url used to authenticate to Salesforce with the OAuth2 password flow.
    /// </summary>
    /// <example>https://mydomain.my.salesforce.com</example>
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
    [RequiredIf(nameof(AuthenticationMethod), AuthenticationMethod.OAuth2WithPassword)]
    [DefaultValue("https://login.salesforce.com")]
    [DisplayFormat(DataFormatString = "Text")]
    public string AuthUrl { get; set; } = "https://login.salesforce.com";

    /// <summary>
    /// Salesforce tenant or org ID. Sent as the tenantId gRPC header.
    /// </summary>
    /// <example>00Dxx0000000001</example>
    [NotEmptyString]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Reusable Salesforce access token. Required when using the AccessToken authentication method.
    /// </summary>
    /// <example>00Dxx0000000001!AQ8AQExampleToken</example>
    [PasswordPropertyText]
    [DefaultValue("")]
    [RequiredIf(nameof(AuthenticationMethod), AuthenticationMethod.AccessToken)]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// OAuth connected app client ID.
    /// </summary>
    /// <example>3MVG9d8..ExampleClientId</example>
    [RequiredIf(
        nameof(AuthenticationMethod),
        AuthenticationMethod.OAuth2WithPassword,
        AuthenticationMethod.OAuth2WithClientCredentials)]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth connected app client secret.
    /// </summary>
    /// <example>ExampleClientSecret</example>
    [PasswordPropertyText]
    [RequiredIf(
        nameof(AuthenticationMethod),
        AuthenticationMethod.OAuth2WithPassword,
        AuthenticationMethod.OAuth2WithClientCredentials)]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Salesforce username.
    /// </summary>
    /// <example>integration.user@example.com</example>
    [RequiredIf(nameof(AuthenticationMethod), AuthenticationMethod.OAuth2WithPassword)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Salesforce password.
    /// </summary>
    /// <example>ExamplePassword</example>
    [PasswordPropertyText]
    [RequiredIf(nameof(AuthenticationMethod), AuthenticationMethod.OAuth2WithPassword)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Salesforce security token appended to the password for the username-password OAuth flow when required.
    /// </summary>
    /// <example>ExampleSecurityToken</example>
    [PasswordPropertyText]
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
    public string SecurityToken { get; set; } = string.Empty;

    /// <summary>
    /// Allows keeping a channel for connection open.
    /// Speed up multiple calls, but needs to be closed at last usage.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ShutdownChannel { get; set; } = true;
}
