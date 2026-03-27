using System;
using dotenv.net;

namespace Frends.Salesforce.PubSubConsume.Tests;

public abstract class TestBase
{
    protected TestBase()
    {
        DotEnv.Load();
        LoginUrl = Environment.GetEnvironmentVariable("LOGIN_URL");
        PubSubApiUrl = Environment.GetEnvironmentVariable("PUBSUB_API_URL");
        InstanceUrl = Environment.GetEnvironmentVariable("INSTANCE_URL");
        TenantId = Environment.GetEnvironmentVariable("TENANT_ID");
        ClientId = Environment.GetEnvironmentVariable("CLIENT_ID");
        ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
        Username = Environment.GetEnvironmentVariable("USERNAME");
        Password = Environment.GetEnvironmentVariable("PASSWORD");
        SecurityToken = Environment.GetEnvironmentVariable("SECURITY_TOKEN");
    }

    protected string LoginUrl { get; }

    protected string PubSubApiUrl { get; }

    protected string InstanceUrl { get; }

    protected string TenantId { get; }

    protected string ClientId { get; }

    protected string ClientSecret { get; }

    protected string Username { get; }

    protected string Password { get; }

    protected string SecurityToken { get; }
}
