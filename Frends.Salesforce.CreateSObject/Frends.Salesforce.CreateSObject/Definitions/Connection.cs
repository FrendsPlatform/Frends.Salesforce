using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.Salesforce.CreateSObject.Attributes;

namespace Frends.Salesforce.CreateSObject.Definitions;

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
    /// Reusable Salesforce access token. Required when using the AccessToken authentication method.
    /// </summary>
    /// <example>00Dxx0000000001!AQ8AQExampleToken</example>
    [PasswordPropertyText]
    [DefaultValue("")]
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.AccessToken)]
    [RequiredIf(nameof(AuthenticationMethod), AuthenticationMethod.AccessToken)]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// OAuth connected app client ID.
    /// </summary>
    /// <example>3MVG9d8..ExampleClientId</example>
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword,
        AuthenticationMethod.OAuth2WithClientCredentials)]
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
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword,
        AuthenticationMethod.OAuth2WithClientCredentials)]
    [RequiredIf(
        nameof(AuthenticationMethod),
        AuthenticationMethod.OAuth2WithPassword,
        AuthenticationMethod.OAuth2WithClientCredentials)]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Salesforce username.
    /// </summary>
    /// <example>integration.user@example.com</example>
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
    [RequiredIf(nameof(AuthenticationMethod), AuthenticationMethod.OAuth2WithPassword)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Salesforce password.
    /// </summary>
    /// <example>ExamplePassword</example>
    [PasswordPropertyText]
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword)]
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
    /// The API version to use when making requests to Salesforce.
    /// If left empty, the default value is v61.0.
    /// </summary>
    [DefaultValue("v61.0")]
    public string ApiVersion { get; set; }

    /// <summary>
    /// Also return access token which is fetched during authentication?
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.OAuth2WithPassword,
        AuthenticationMethod.OAuth2WithClientCredentials)]
    public bool ReturnAccessToken { get; set; }
}
