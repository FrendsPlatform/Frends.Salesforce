namespace Frends.Salesforce.Toolkit.Definitions;

public interface IRestConnection : IConnection
{
    string ApiVersion { get; set; }
}
