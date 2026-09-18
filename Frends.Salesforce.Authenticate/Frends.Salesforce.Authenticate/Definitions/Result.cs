namespace Frends.Salesforce.Authenticate.Definitions;

/// <summary>
/// Result class usually contains properties of the return object.
/// </summary>
public class Result
{
    internal Result(string accessToken, bool success)
    {
        Success = success;
        this.AccessToken = accessToken;
    }

    /// <summary>
    /// Access token created.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Indicates whether the authentication was successful.
    /// </summary>
    /// <example>00Dxx0000001gXXX!AQkAQF4JkV.NyX3yo65SDfrD_Pkq9</example>
    public string AccessToken { get; private set; }
}
