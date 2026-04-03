using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.PubSubPublish.Definitions;
using Frends.Salesforce.PubSubPublish.Helpers;
using NUnit.Framework;

namespace Frends.Salesforce.PubSubPublish.Tests.HandlersTests;

public class ConnectionHandlerTests : TestBase
{
    private static Connection connection;

    [SetUp]
    public void Setup()
    {
        connection = new Connection
        {
            InstanceUrl = InstanceUrl,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            Username = Username,
            Password = Password,
            SecurityToken = SecurityToken,
        };
    }

    [Test]
    [TestCase(AuthenticationMethod.OAuth2WithPassword)]
    [TestCase(AuthenticationMethod.OAuth2WithClientCredentials)]
    [TestCase(AuthenticationMethod.AccessToken)]
    public async Task GetAccessTokenTest(AuthenticationMethod authenticationMethod)
    {
        connection.AuthenticationMethod = authenticationMethod;
        var token = await ConnectionHandler.GetAccessToken(connection, CancellationToken.None);
        Assert.That(token, Is.Not.Null);
    }

    [Test]
    public void ThrowsWhenInvalidPassword()
    {
        connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        connection.Password = "invalid";
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await ConnectionHandler.GetAccessToken(connection, CancellationToken.None));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message.Contains("Failed to obtain access token"), Is.True);
    }

    [Test]
    public void ThrowsWhenInvalidUsername()
    {
        connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        connection.Username = "invalid";
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await ConnectionHandler.GetAccessToken(connection, CancellationToken.None));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message.Contains("Failed to obtain access token"), Is.True);
    }

    [Test]
    public async Task ReturnsProvidedAccessToken()
    {
        connection.AuthenticationMethod = AuthenticationMethod.AccessToken;
        connection.AccessToken = "invalid";
        var token = await ConnectionHandler.GetAccessToken(connection, CancellationToken.None);
        Assert.That(token.Equals("invalid"), Is.True);
    }

    [Test]
    public void ThrowsWhenInvalidClientId()
    {
        connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        connection.ClientId = "invalid";
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await ConnectionHandler.GetAccessToken(connection, CancellationToken.None));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message.Contains("Failed to obtain access token"), Is.True);
    }

    [Test]
    public void ThrowsWhenInvalidClientSecret()
    {
        connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        connection.ClientSecret = "invalid";

        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await ConnectionHandler.GetAccessToken(connection, CancellationToken.None));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message.Contains("Failed to obtain access token"), Is.True);
    }

    [Test]
    public void ThrowsWhenInvalidSecretToken()
    {
        connection.AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword;
        connection.SecurityToken = "invalid";
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await ConnectionHandler.GetAccessToken(connection, CancellationToken.None));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message.Contains("Failed to obtain access token"), Is.True);
    }
}
