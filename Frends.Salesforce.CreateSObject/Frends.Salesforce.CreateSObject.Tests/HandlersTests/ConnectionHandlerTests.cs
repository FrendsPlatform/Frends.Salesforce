using System;
using System.Threading;
using System.Threading.Tasks;
using dotenv.net;
using Frends.Salesforce.CreateSObject.Definitions;
using Frends.Salesforce.CreateSObject.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.Salesforce.CreateSObject.Tests.HandlersTests;

[TestClass]
public class ConnectionHandlerTests
{
    private static string _clientSecret;
    private static string _password;
    private static string _securityToken;
    private static string _clientID;
    private static string _username;
    private static string _domain;
    private static string _token;
    private static Connection _connection;


    [ClassInitialize]
    public static async Task ClassInitialize(TestContext testContext)
    {
        DotEnv.Load();
        _clientSecret = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_SECRET");
        _password = Environment.GetEnvironmentVariable("SALESFORCE_PASSWORD");
        _securityToken = Environment.GetEnvironmentVariable("SALESFORCE_SECURITY_TOKEN");
        _clientID = Environment.GetEnvironmentVariable("SALESFORCE_CLIENTID");
        _username = Environment.GetEnvironmentVariable("SALESFORCE_USERNAME");
        _domain = Environment.GetEnvironmentVariable("SALESFORCE_DOMAIN_URL");

        _token = await TestHelper.GetAccessToken(_domain, _clientID, _clientSecret);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _connection = new Connection
        {
            InstanceUrl = _domain,
            AccessToken = _token,
            ApiVersion = "v61.0",
            ClientId = _clientID,
            ClientSecret = _clientSecret,
            Username = _username,
            Password = _password,
            SecurityToken = _securityToken,
        };
    }

    [TestMethod]
    [DataRow(AuthenticationMethod.OAuth2WithPassword)]
    [DataRow(AuthenticationMethod.OAuth2WithClientCredentials)]
    [DataRow(AuthenticationMethod.AccessToken)]
    public async Task GetAccessTokenTest(AuthenticationMethod authenticationMethod)
    {
        _connection.AuthenticationMethod = authenticationMethod;
        var token = await ConnectionHandler.GetAccessToken(_connection, CancellationToken.None);
        Assert.IsNotNull(token);
    }

    [TestMethod]
    public async Task ThrowsWhenInvalidPassword()
    {
        _connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        _connection.Password = "invalid";
        var ex = await Assert.ThrowsExactlyAsync<Exception>(() =>
            ConnectionHandler.GetAccessToken(_connection, CancellationToken.None));
        Assert.IsTrue(ex.Message.Contains("Failed to obtain access token"));
    }

    [TestMethod]
    public async Task ThrowsWhenInvalidUsername()
    {
        _connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        _connection.Username = "invalid";
        var ex = await Assert.ThrowsExactlyAsync<Exception>(() =>
            ConnectionHandler.GetAccessToken(_connection, CancellationToken.None));
        Assert.IsTrue(ex.Message.Contains("Failed to obtain access token"));
    }

    [TestMethod]
    public async Task ReturnsProvidedAccessToken()
    {
        _connection.AuthenticationMethod = AuthenticationMethod.AccessToken;
        _connection.AccessToken = "invalid";
        var token = await ConnectionHandler.GetAccessToken(_connection, CancellationToken.None);
        Assert.AreEqual("invalid", token);
    }

    [TestMethod]
    public async Task ThrowsWhenInvalidClientId()
    {
        _connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        _connection.ClientId = "invalid";
        var ex = await Assert.ThrowsExactlyAsync<Exception>(() =>
            ConnectionHandler.GetAccessToken(_connection, CancellationToken.None));
        Assert.IsTrue(ex.Message.Contains("Failed to obtain access token"));
    }

    [TestMethod]
    public async Task ThrowsWhenInvalidClientSecret()
    {
        _connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        _connection.ClientSecret = "invalid";
        var ex = await Assert.ThrowsExactlyAsync<Exception>(() =>
            ConnectionHandler.GetAccessToken(_connection, CancellationToken.None));
        Assert.IsTrue(ex.Message.Contains("Failed to obtain access token"));
    }

    [TestMethod]
    public async Task ThrowsWhenInvalidSecretToken()
    {
        _connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        _connection.SecurityToken = "invalid";
        var ex = await Assert.ThrowsExactlyAsync<Exception>(() =>
            ConnectionHandler.GetAccessToken(_connection, CancellationToken.None));
        Assert.IsTrue(ex.Message.Contains("Failed to obtain access token"));
    }

    [TestMethod]
    public void ShouldReturnToken_IsFalse_When_AccessTokenMethod()
    {
        var result = ConnectionHandler.ShouldReturnToken(AuthenticationMethod.AccessToken, true);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ShouldReturnToken_IsFalse_When_FlagIsFalse()
    {
        var result = ConnectionHandler.ShouldReturnToken(AuthenticationMethod.OAuth2WithPassword, false);
        Assert.IsFalse(result);
    }


}
