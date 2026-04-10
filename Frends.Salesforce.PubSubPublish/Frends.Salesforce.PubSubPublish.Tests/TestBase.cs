using System;
using dotenv.net;

namespace Frends.Salesforce.PubSubPublish.Tests;

public abstract class TestBase
{
    protected TestBase()
    {
        DotEnv.Load();
        PubSubApiUrl = Environment.GetEnvironmentVariable("SALESFORCE_PUBSUB_API_URL");
        InstanceUrl = Environment.GetEnvironmentVariable("SALESFORCE_INSTANCE_URL");
        TenantId = Environment.GetEnvironmentVariable("SALESFORCE_TENANT_ID");
        ClientId = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_ID");
        ClientSecret = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_SECRET");
        Username = Environment.GetEnvironmentVariable("SALESFORCE_USERNAME");
        Password = Environment.GetEnvironmentVariable("SALESFORCE_PASSWORD");
        SecurityToken = Environment.GetEnvironmentVariable("SALESFORCE_SECURITY_TOKEN");
    }

    protected string PubSubApiUrl { get; }

    protected string InstanceUrl { get; }

    protected string TenantId { get; }

    protected string ClientId { get; }

    protected string ClientSecret { get; }

    protected string Username { get; }

    protected string Password { get; }

    protected string SecurityToken { get; }
}
