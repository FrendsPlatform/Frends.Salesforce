using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using dotenv.net;
using Frends.Salesforce.ExecuteQuery.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.Salesforce.ExecuteQuery.Tests;

[TestClass]
public class UnitTests
{
    private static string clientSecret;
    private static string password;
    private static string securityToken;
    private static string clientId;
    private static string username;
    private static string domain;
    private static string token;

    private readonly CancellationToken cancellationToken = CancellationToken.None;

    [ClassInitialize]
    public static async Task TestInitialize(TestContext testContext)
    {
        DotEnv.Load();
        clientSecret = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_SECRET");
        password = Environment.GetEnvironmentVariable("SALESFORCE_PASSWORD");
        securityToken = Environment.GetEnvironmentVariable("SALESFORCE_SECURITY_TOKEN");
        clientId = Environment.GetEnvironmentVariable("SALESFORCE_CLIENTID");
        username = Environment.GetEnvironmentVariable("SALESFORCE_USERNAME");
        domain = Environment.GetEnvironmentVariable("SALESFORCE_DOMAIN_URL");

        token = await TestHelper.GetAccessToken(domain, clientId, clientSecret);
    }

    [TestMethod]
    public async Task ExecuteQuery_QueryWithToken()
    {
        var input = new Input
        {
            Query = "SELECT Name from Customer",
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        var result = await Salesforce.ExecuteQuery(input, con, new Options(), cancellationToken);
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public async Task ExecuteQuery_QueryWithPassword()
    {
        var input = new Input
        {
            Query = "SELECT Name from Customer",
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Username = username,
            Password = password,
            SecurityToken = securityToken,
        };

        var result = await Salesforce.ExecuteQuery(input, con, new Options(), cancellationToken);
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public async Task ExecuteQuery_QueryWithPassword_ReturnToken()
    {
        var input = new Input
        {
            Query = "SELECT Name from Customer",
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Username = username,
            Password = password,
            SecurityToken = securityToken,
            ReturnAccessToken = true,
        };

        var result = await Salesforce.ExecuteQuery(input, con, new Options(), cancellationToken);
        Assert.IsTrue(result.Success);
        Assert.IsNotEmpty(result.Token);
    }

    [TestMethod]
    public async Task ExecuteQuery_QueryWithoutSpecifiedApi()
    {
        var input = new Input
        {
            Query = "SELECT Name from Customer",
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        var result = await Salesforce.ExecuteQuery(input, con, new Options(), cancellationToken);
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    [ExpectedException(typeof(HttpRequestException))]
    public async Task InvalidDomain_ThrowTest()
    {
        var input = new Input
        {
            Query = "SELECT Name from Customer",
        };

        var con = new Connection
        {
            InstanceUrl = "https://invalid-mycompany.my.salesforce.com",
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        await Salesforce.ExecuteQuery(input, con, new Options(), cancellationToken);
    }

    [TestMethod]
    public async Task InvalidQuery_ReturnsError()
    {
        var input = new Input
        {
            Query = "SELECT NAME from Invalid",
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        var result = await Salesforce.ExecuteQuery(input, con, new Options { ThrowErrorOnFailure = false }, cancellationToken);
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        StringAssert.Contains(result.Error.Message, "sObject type 'Invalid' is not supported.");
    }

    [TestMethod]
    public async Task ExecuteQuery_WithClientCredentials()
    {
        var input = new Input
        {
            Query = "SELECT Name from Customer",
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            ClientId = clientId,
            ClientSecret = clientSecret,
        };

        var result = await Salesforce.ExecuteQuery(input, con, new Options(), cancellationToken);
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public async Task ExecuteQuery_WithClientCredentials_ReturnToken()
    {
        var input = new Input
        {
            Query = "SELECT Name from Customer",
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            ClientId = clientId,
            ClientSecret = clientSecret,
            ReturnAccessToken = true,
        };

        var result = await Salesforce.ExecuteQuery(input, con, new Options(), cancellationToken);
        Assert.IsTrue(result.Success);
        Assert.IsNotEmpty(result.Token);
    }
}
