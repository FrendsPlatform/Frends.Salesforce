namespace Frends.Salesforce.CreateSObject.Definitions;
/// <summary>
/// Enums-class for CreateSObject-task.
/// </summary>
public class Enums
{
    /// <summary>
    /// Authentication options to authenticate to Salesforce.
    /// </summary>
    /// <example>AccessToken</example>
    public enum AuthenticationMethod
    {
        /// <summary>
        /// Authenticate with access token.
        /// </summary>
        AccessToken,
        /// <summary>
        /// Authenticate by providing required informations to fetch OAuth2 access token.
        /// </summary>
        OAuth2WithPassword
    }
}
