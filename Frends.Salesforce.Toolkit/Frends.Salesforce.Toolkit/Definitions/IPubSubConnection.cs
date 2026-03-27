namespace Frends.Salesforce.Toolkit.Definitions;

public interface IPubSubConnection : IConnection
{
    string PubSubApiUrl { get; set; }
    string TenantId { get; set; }
    bool ShutdownChannel { get; set; }
}
