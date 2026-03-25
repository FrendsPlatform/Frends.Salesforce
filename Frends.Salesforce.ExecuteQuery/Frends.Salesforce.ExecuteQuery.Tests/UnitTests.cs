using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.ExecuteQuery.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dotenv.net;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;

namespace Frends.Salesforce.ExecuteQuery.Tests;

[TestClass]
public class UnitTests
{
    private readonly string _clientSecret = Environment.GetEnvironmentVariable("Salesforce_Client_Secret");
    private readonly string _password = Environment.GetEnvironmentVariable("Salesforce_Password");
    private readonly string _securityToken = Environment.GetEnvironmentVariable("Salesforce_Security_Token");
    private readonly string _clientID = Environment.GetEnvironmentVariable("Salesforce_ClientID");
    private readonly string _username = Environment.GetEnvironmentVariable("Salesforce_Username");
    private readonly string _domain = Environment.GetEnvironmentVariable("Salesforce_Domain_Url");
    private readonly string _authurl = Environment.GetEnvironmentVariable("Salesforce_Auth_Url");
    private static string _authUrlForOAuth2WithCredentials;

    private readonly CancellationToken _cancellationToken = new();

    [ClassInitialize]
    public static void TestInitialize(TestContext testContext)
    {
        DotEnv.Load();
        _authUrlForOAuth2WithCredentials =
            Environment.GetEnvironmentVariable("Salesforce_Domain_Url") + "/services/oauth2/token";
    }

    [TestMethod]
    public async Task ExecuteQuery_QueryWithToken()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = "SELECT Name from Customer",
            ApiVersion = "v61.0"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username,
                _password + _securityToken, _cancellationToken)
        };

        var result = await Salesforce.ExecuteQuery(input, options, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task ExecuteQuery_QueryWithClientCredentials()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = "SELECT Name from Customer",
            ApiVersion = "v61.0"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            AuthUrl = _authUrlForOAuth2WithCredentials,
            ClientID = _clientID,
            ClientSecret = _clientSecret
        };

        var result = await Salesforce.ExecuteQuery(input, options, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task ExecuteQuery_QueryWithClientCredentials_ReturnToken()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = "SELECT Name from Customer",
            ApiVersion = "v61.0"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            AuthUrl = _authUrlForOAuth2WithCredentials,
            ClientID = _clientID,
            ClientSecret = _clientSecret,
            ReturnAccessToken = true
        };

        var result = await Salesforce.ExecuteQuery(input, options, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
        Assert.IsNotEmpty(result.Token);
    }

    [TestMethod]
    public async Task ExecuteQuery_QueryWithPassword()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = "SELECT Name from Customer",
            ApiVersion = "v61.0"
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
            Query = "SELECT Name from Customer",
            ApiVersion = "v61.0"
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
        var accessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username,
            _password + _securityToken, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
        Assert.AreEqual(result.Token, accessToken);
    }

    public async Task ExecuteQuery_QueryWithoutSpecifiedApi()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = "SELECT Name from Customer",
            ApiVersion = ""
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username,
                _password + _securityToken, _cancellationToken)
        };

        var result = await Salesforce.ExecuteQuery(input, options, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
    }


    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task EmptyQuery_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = null,
            ApiVersion = "v61.0"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username,
                _password + _securityToken, _cancellationToken)
        };

        await Salesforce.ExecuteQuery(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task EmptyAccessToken_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = "SELECT Name from Customer",
            ApiVersion = "v61.0"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = " "
        };

        await Salesforce.ExecuteQuery(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task EmptyDomain_ThrowTest()
    {
        var input = new Input
        {
            Domain = null,
            Query = "SELECT Name from Customer",
            ApiVersion = "v61.0"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username,
                _password + _securityToken, _cancellationToken)
        };

        await Salesforce.ExecuteQuery(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task InvalidDomain_ThrowTest()
    {
        var input = new Input
        {
            Domain = "https://mycompany.my.salesforce.com",
            Query = "SELECT Name from Customer",
            ApiVersion = "v61.0"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username,
                _password + _securityToken, _cancellationToken)
        };

        await Salesforce.ExecuteQuery(input, options, _cancellationToken);
    }

    [TestMethod]
    public async Task InvalidQuery_ReturnsError()
    {
        var input = new Input
        {
            Domain = _domain,
            Query = "SELECT NAME from Invalid",
            ApiVersion = "v61.0"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username,
                _password + _securityToken, _cancellationToken)
        };

        var result = await Salesforce.ExecuteQuery(input, options, _cancellationToken);
        Assert.IsNotNull(result.ErrorMessage);
        StringAssert.Contains(result.ErrorMessage, "sObject type 'Invalid' is not supported.");
    }
}
