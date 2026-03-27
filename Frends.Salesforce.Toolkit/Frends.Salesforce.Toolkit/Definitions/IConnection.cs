namespace Frends.Salesforce.Toolkit.Definitions;

public interface IConnection
{
    AuthenticationMethod AuthenticationMethod { get; set; }

    string AccessToken { get; set; }
    string InstanceUrl { get; set; }
    string AuthUrl { get; set; }
    string ClientId { get; set; }
    string ClientSecret { get; set; }
    string Username { get; set; }
    string Password { get; set; }
    string SecurityToken { get; set; }
}
