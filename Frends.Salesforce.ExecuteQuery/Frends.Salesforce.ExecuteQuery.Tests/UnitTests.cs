using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.ExecuteQuery.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dotenv.net;
using NUnit.Framework.Internal;

namespace Frends.Salesforce.ExecuteQuery.Tests;
[TestClass]
public class UnitTests
{
    private readonly string _clientSecret = Environment.GetEnvironmentVariable("Salesforce_Client_Secret");
    private readonly string _password = Environment.GetEnvironmentVariable("Salesforce_Password");
    private readonly string _securityToken = Environment.GetEnvironmentVariable("Salesforce_Security_Token");
    private readonly string _clientID = Environment.GetEnvironmentVariable("Salesforce_ClientID");
    private readonly string _username = Environment.GetEnvironmentVariable("Salesforce_Username");
    private readonly string _domain = @"https://frends2-dev-ed.develop.my.salesforce.com";
    private readonly string _authurl = @"https://login.salesforce.com/services/oauth2/token";
    private readonly CancellationToken _cancellationToken = new();

    [ClassInitialize]
    public static void TestInitialize(TestContext testContext)
    {
        // load envs
        var root = Directory.GetCurrentDirectory();
        var projDir = Directory.GetParent(root)?.Parent?.Parent?.FullName;
        DotEnv.Load(
            options: new DotEnvOptions(
                envFilePaths: new[] { $"{projDir}{Path.DirectorySeparatorChar}.env.local" }));   
    }

    [TestMethod]
    public async Task ExecuteQuery_QueryWithToken()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = "SELECT Name from Customer"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        var result = await Salesforce.ExecuteQuery(input, options, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task ExecuteQuery_QueryWithPassword()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = "SELECT Name from Customer"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            AuthUrl = _authurl,
            ClientID = _clientID,
            ClientSecret = _clientSecret,
            Username = _username,
            Password = _password,
            SecurityToken = _securityToken
        };

        var result = await Salesforce.ExecuteQuery(input, options, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task ExecuteQuery_QueryWithPassword_ReturnToken()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = "SELECT Name from Customer"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            AuthUrl = _authurl,
            ClientID = _clientID,
            ClientSecret = _clientSecret,
            Username = _username,
            Password = _password,
            SecurityToken = _securityToken,
            ReturnAccessToken = true
        };

        var result = await Salesforce.ExecuteQuery(input, options, _cancellationToken);
        var accessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
        Assert.AreEqual(result.Token, accessToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task EmptyQuery_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = null
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.ExecuteQuery(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task EmptyAccessToken_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = "SELECT Name from Customer"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = " "
        };

        await Salesforce.ExecuteQuery(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task EmptyDomain_ThrowTest()
    {
        var input = new Input
        {
            Domain = null,
            Query = "SELECT Name from Customer"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.ExecuteQuery(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task InvalidDomain_ThrowTest()
    {
        var input = new Input
        {
            Domain = "https://mycompany.my.salesforce.com",
            Query = "SELECT Name from Customer"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.ExecuteQuery(input, options, _cancellationToken);
    }

    [TestMethod]
    public async Task InvalidQuery_ReturnsError()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = "SELECT NAME from Invalid"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        var result = await Salesforce.ExecuteQuery(input, options, _cancellationToken);
        Assert.IsNotNull(result.ErrorMessage);
        StringAssert.Contains(result.ErrorMessage, "sObject type 'Invalid' is not supported.");
    }
}
