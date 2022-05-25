using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.DeleteSObject.Definitions;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Frends.Salesforce.DeleteSObject.Tests;

[TestClass]
public class UnitTests
{
    private readonly string _clientSecret = Environment.GetEnvironmentVariable("Salesforce_Client_Secret");
    private readonly string _password = Environment.GetEnvironmentVariable("Salesforce_Password");
    private readonly string _securityToken = Environment.GetEnvironmentVariable("Salesforce_Security_Token");
    private readonly string _clientID = Environment.GetEnvironmentVariable("Salesforce_ClientID");

    private readonly string _domain = @"https://hiqfinlandoy2-dev-ed.my.salesforce.com";
    private readonly string _username = "testuser@test.fi";
    private readonly string _authurl = @"https://login.salesforce.com/services/oauth2/token";

    private readonly CancellationToken _cancellationToken = new();
    private Options _options;
    private string _userJson;

    readonly string _name = "Test" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Millisecond;

    [TestInitialize]
    public async Task TestInitialize()
    {
        _userJson = JsonSerializer.Serialize(new { Name = _name });

        _options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };
    }

    [TestMethod]
    public async Task DeleteAccountTest()
    {
        var id = await CreateSObject("Account", _userJson);
        var result = await Salesforce.DeleteSObject(new Input { Domain = _domain, SObjectId = id, SObjectType = "Account" }, _options, _cancellationToken);

        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task DeleteContactTest() {
        var json = JsonSerializer.Serialize(
            new
            {
                Title = "Mr",
                LastName = _name
            });

        var id = await CreateSObject("Contact", json);
        var result = await Salesforce.DeleteSObject(new Input { Domain = _domain, SObjectId = id, SObjectType = "Contact" }, _options, _cancellationToken);

        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task DeleteCaseTest()
    {
        // Creating an account to which case can be linked to.
        var accountId = await CreateSObject("Account", _userJson);

        // Creating a case.
        var json = JsonSerializer.Serialize(new {
            AccountId = accountId,
            Subject = "This is a test.",
            Description = "This is a test case for Frends.SalesForce.CreateSObject task.",
            Origin = "Web"
        });

        var caseId = await CreateSObject("Case", json);

        var caseResult = await Salesforce.DeleteSObject(new Input { Domain = _domain, SObjectId = caseId, SObjectType = "Case" }, _options, _cancellationToken);
        Assert.IsTrue(caseResult.RequestIsSuccessful);

        var accountResult = await Salesforce.DeleteSObject(new Input { Domain = _domain, SObjectId = accountId, SObjectType = "Account" }, _options, _cancellationToken);
        Assert.IsTrue(accountResult.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task GetReturnedAccessTokenTest()
    {
        var id = await CreateSObject("Account", _userJson);
        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            AuthUrl = _authurl,
            ClientID = _clientID,
            ClientSecret = _clientSecret,
            Username = _username,
            Password = _password + _securityToken,
            ReturnAccessToken = true
        };
        var result = await Salesforce.DeleteSObject(new Input { Domain = _domain, SObjectId = id, SObjectType = "Account" }, options, _cancellationToken);

        Assert.IsNotNull(result.Token);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task EmptyAccessToken_ThrowTest() {
        var input = new Input
        {
            Domain = _domain,
            SObjectId = "123456789",
            SObjectType = "Contact"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = " "
        };

        await Salesforce.DeleteSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task EmptyDomain_ThrowTest()
    {
        var input = new Input
        {
            Domain = null,
            SObjectId = "123456789",
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.DeleteSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task EmptyId_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            SObjectId = null,
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.DeleteSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task EmptyType_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            SObjectId = "123456789",
            SObjectType = ""
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.DeleteSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task InvalidDomain_ThrowTest()
    {
        var input = new Input
        {
            Domain = "https://mycompany.my.salesforce.com",
            SObjectId = "123456789",
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            AuthUrl = _authurl,
            ClientID = _clientID,
            ClientSecret = _clientSecret,
            Username = _username,
            Password = _password + _securityToken,
        };

        await Salesforce.DeleteSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    public async Task InvalidObjectType_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            SObjectId = "123456789",
            SObjectType = "InvalidType"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            AuthUrl = _authurl,
            ClientID = _clientID,
            ClientSecret = _clientSecret,
            Username = _username,
            Password = _password + _securityToken,
        };

        var result = await Salesforce.DeleteSObject(input, options, _cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code NotFound").ToString(), result.ErrorException.ToString());
    }

    [TestMethod]
    public async Task InvalidSecretOAuth_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            SObjectId = "123456789",
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            AuthUrl = _authurl,
            ClientID = _clientID,
            ClientSecret = "abcdefghijklmn123456789",
            Username = _username,
            Password = _password + _securityToken,
        };

        var result = await Salesforce.DeleteSObject(input, options, _cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code Unauthorized").ToString(), result.ErrorException.ToString());
    }

    [TestMethod]
    public async Task InvalidId_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            SObjectId = "Not valid id",
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        var result = await Salesforce.DeleteSObject(input, options, _cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code NotFound").ToString(), result.ErrorException.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(HttpRequestException))]
    public async Task NotFoundId_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            SObjectId = "123456789",
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken),
            ThrowAnErrorIfNotFound = true
        };

        await Salesforce.DeleteSObject(input, options, _cancellationToken);
    }

        // Helper method to create SObjects for delete function.
        private async Task<string> CreateSObject(string type, string input)
    {
        var client = new RestClient(_domain + "/services/data/v54.0/sobjects/" + type);
        var request = new RestRequest("/", Method.Post);

        var accessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken);
        request.AddHeader("Authorization", "Bearer " + accessToken);

        var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(input);
        request.RequestFormat = DataFormat.Json;
        request.AddJsonBody(json);

        var response = await client.ExecuteAsync(request, _cancellationToken);
        var content = JsonConvert.DeserializeObject<dynamic>(response.Content);

        return content.id.ToString();
    }
}

