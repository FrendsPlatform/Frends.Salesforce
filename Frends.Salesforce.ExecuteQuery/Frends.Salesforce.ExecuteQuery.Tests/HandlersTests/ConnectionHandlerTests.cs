using System;
using System.Threading;
using System.Threading.Tasks;
using dotenv.net;
using Frends.Salesforce.ExecuteQuery.Definitions;
using Frends.Salesforce.ExecuteQuery.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.Salesforce.ExecuteQuery.Tests.HandlersTests;

[TestClass]
public class ConnectionHandlerTests
{
    private static string clientSecret;
    private static string password;
    private static string securityToken;
    private static string clientId;
    private static string username;
    private static string domain;
    private static Connection connection;


    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
        DotEnv.Load();
        clientSecret = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_SECRET");
        password = Environment.GetEnvironmentVariable("SALESFORCE_PASSWORD");
        securityToken = Environment.GetEnvironmentVariable("SALESFORCE_SECURITY_TOKEN");
        clientId = Environment.GetEnvironmentVariable("SALESFORCE_CLIENTID");
        username = Environment.GetEnvironmentVariable("SALESFORCE_USERNAME");
        domain = Environment.GetEnvironmentVariable("SALESFORCE_DOMAIN_URL");
    }

    [TestInitialize]
    public void TestInitialize()
    {
        connection = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            ClientId = clientId,
            ClientSecret = clientSecret,
            Username = username,
            Password = password,
            SecurityToken = securityToken,
        };
    }

    [TestMethod]
    [DataRow(AuthenticationMethod.OAuth2WithPassword)]
    [DataRow(AuthenticationMethod.OAuth2WithClientCredentials)]
    [DataRow(AuthenticationMethod.AccessToken)]
    public async Task GetAccessTokenTest(AuthenticationMethod authenticationMethod)
    {
        connection.AuthenticationMethod = authenticationMethod;
        var token = await ConnectionHandler.GetAccessToken(connection, CancellationToken.None);
        Assert.IsNotNull(token);
    }

    [TestMethod]
    public async Task ThrowsWhenInvalidPassword()
    {
        connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        connection.Password = "invalid";
        var ex = await Assert.ThrowsExactlyAsync<Exception>(() =>
            ConnectionHandler.GetAccessToken(connection, CancellationToken.None));
        Assert.IsTrue(ex.Message.Contains("Failed to obtain access token"));
    }

    [TestMethod]
    public async Task ThrowsWhenInvalidUsername()
    {
        connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        connection.Username = "invalid";
        var ex = await Assert.ThrowsExactlyAsync<Exception>(() =>
            ConnectionHandler.GetAccessToken(connection, CancellationToken.None));
        Assert.IsTrue(ex.Message.Contains("Failed to obtain access token"));
    }

    [TestMethod]
    public async Task ReturnsProvidedAccessToken()
    {
        connection.AuthenticationMethod = AuthenticationMethod.AccessToken;
        connection.AccessToken = "invalid";
        var token = await ConnectionHandler.GetAccessToken(connection, CancellationToken.None);
        Assert.AreEqual("invalid", token);
    }

    [TestMethod]
    public async Task ThrowsWhenInvalidClientId()
    {
        connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        connection.ClientId = "invalid";
        var ex = await Assert.ThrowsExactlyAsync<Exception>(() =>
            ConnectionHandler.GetAccessToken(connection, CancellationToken.None));
        Assert.IsTrue(ex.Message.Contains("Failed to obtain access token"));
    }

    [TestMethod]
    public async Task ThrowsWhenInvalidClientSecret()
    {
        connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        connection.ClientSecret = "invalid";
        var ex = await Assert.ThrowsExactlyAsync<Exception>(() =>
            ConnectionHandler.GetAccessToken(connection, CancellationToken.None));
        Assert.IsTrue(ex.Message.Contains("Failed to obtain access token"));
    }

    [TestMethod]
    public async Task ThrowsWhenInvalidSecretToken()
    {
        connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        connection.SecurityToken = "invalid";
        var ex = await Assert.ThrowsExactlyAsync<Exception>(() =>
            ConnectionHandler.GetAccessToken(connection, CancellationToken.None));
        Assert.IsTrue(ex.Message.Contains("Failed to obtain access token"));
    }

    [TestMethod]
    public void ShouldReturnToken_IsFalse_When_AccessTokenMethod()
    {
        var result  =  ConnectionHandler.ShouldReturnToken(AuthenticationMethod.AccessToken, true);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ShouldReturnToken_IsFalse_When_FlagIsFalse()
    {
        var result  =  ConnectionHandler.ShouldReturnToken(AuthenticationMethod.OAuth2WithPassword, false);
        Assert.IsFalse(result);
    }


}
