using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.CreateSObject.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;
using dotenv.net;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Frends.Salesforce.CreateSObject.Tests;
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
    private Options _options;
    private string _userJson;
    private List<object> _result;

    private string _name = "Test" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Millisecond;

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
        // load envs
        var root = Directory.GetCurrentDirectory();
        var projDir = Directory.GetParent(root)?.Parent?.Parent?.FullName;
        DotEnv.Load(
            options: new DotEnvOptions(
                envFilePaths: new[] { $"{projDir}{Path.DirectorySeparatorChar}.env.local" }));
    }

    [TestInitialize]
    public async Task TestInitialize()
    {
        _result = new List<object>();

        _userJson = JsonSerializer.Serialize(new { Name = _name });

        _options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };
    }

    [TestCleanup]
    public async Task TestCleanUp()
    {
        if (_result != null)
        {
            for (var i = (_result.Count-1); i >= 0; i--) {
                var temp = JsonConvert.SerializeObject(_result[i]);
                var obj = JsonConvert.DeserializeObject<dynamic>(temp);

                var client = new RestClient(_domain + "/services/data/v54.0/sobjects/" + obj.Type + "/" + obj.Id);
                var request = new RestRequest("/", Method.Delete);

                request.AddHeader("Authorization", "Bearer " + _options.AccessToken);
                await client.ExecuteAsync(request, _cancellationToken);
            }
            _result = null;
        }
    }

    [TestMethod]
    public async Task CreateAccountTest()
    {
        var input = new Input
        {
            Domain = _domain,
            SObjectAsJson = _userJson,
            SObjectType = "Account"
        };

        var result = await Salesforce.CreateSObject(input, _options, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);

        var body = JsonConvert.SerializeObject(result.Body);
        var obj = JsonConvert.DeserializeObject<dynamic>(body);
        _result.Add(new { Type = "Account", Id = obj.id });
    }

    [TestMethod]
    public async Task CreateContactTest() {
        var json = JsonSerializer.Serialize(
            new
            {
                Title = "Mr",
                LastName = _name
            });

        var input = new Input
        {
            Domain = _domain,
            SObjectAsJson = json,
            SObjectType = "Contact"
        };

        var result = await Salesforce.CreateSObject(input, _options, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);

        var body = JsonConvert.SerializeObject(result.Body);
        var obj = JsonConvert.DeserializeObject<dynamic>(body);
        _result.Add(new { Type = "Contact", Id = obj.id });
    }

    [TestMethod]
    public async Task CreateCaseTest()
    {
        // Creating an account to which case can be linked to.
        var accountInput = new Input
        {
            Domain = _domain,
            SObjectAsJson = _userJson,
            SObjectType = "Account"
        };

        var accountResult = await Salesforce.CreateSObject(accountInput, _options, _cancellationToken);

        var body = JsonConvert.SerializeObject(accountResult.Body);
        var accObj = JsonConvert.DeserializeObject<dynamic>(body);
        _result.Add(new { Type = "Account", Id = accObj.id });

        // Creating a case.
        var json = JsonSerializer.Serialize(new {
            AccountId = accObj.id.ToString(),
            Subject = "This is a test.",
            Description = "This is a test case for Frends.SalesForce.CreateSObject task.",
            Origin = "Web"
        });

        var caseInput = new Input
        {
            Domain = _domain,
            SObjectAsJson = json,
            SObjectType = "Case"
        };

        var caseResult = await Salesforce.CreateSObject(caseInput, _options, _cancellationToken);
        Assert.IsTrue(caseResult.RequestIsSuccessful);

        var caseBody = JsonConvert.SerializeObject(caseResult.Body);
        var caseObj = JsonConvert.DeserializeObject<dynamic>(caseBody);
        _result.Add(new { Type = "Case", Id = caseObj.id });
    }

    [TestMethod]
    public async Task GetReturnedAccessTokenTest()
    {
        var input = new Input
        {
            Domain = _domain,
            SObjectAsJson = _userJson,
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
            ReturnAccessToken = true
        };
        var result = await Salesforce.CreateSObject(input, options, _cancellationToken);
        Assert.IsNotNull(result.Token);

        var body = JsonConvert.SerializeObject(result.Body);
        var obj = JsonConvert.DeserializeObject<dynamic>(body);
        _result.Add(new { Type = "Account", Id = obj.id });
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task EmptyAccessToken_ThrowTest() {
        var input = new Input
        {
            Domain = _domain,
            SObjectAsJson = _userJson,
            SObjectType = "Contact"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = " "
        };

        await Salesforce.CreateSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task EmptyDomain_ThrowTest()
    {
        var input = new Input
        {
            Domain = null,
            SObjectAsJson = _userJson,
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.CreateSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task EmptyJson_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            SObjectAsJson = null,
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.CreateSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task EmptyType_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            SObjectAsJson = _userJson,
            SObjectType = ""
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.CreateSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task InvalidDomain_ThrowTest()
    {
        var input = new Input
        {
            Domain = "https://mycompany.my.salesforce.com",
            SObjectAsJson = _userJson,
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

        await Salesforce.CreateSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    public async Task InvalidObjectType_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            SObjectAsJson = _userJson,
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

        var result = await Salesforce.CreateSObject(input, options, _cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code NotFound").ToString(), result.ErrorException.ToString());
    }

    [TestMethod]
    public async Task InvalidSecretOAuth_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            SObjectAsJson = _userJson,
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

        var result = await Salesforce.CreateSObject(input, options, _cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code Unauthorized").ToString(), result.ErrorException.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public async Task InvalidJson_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            SObjectAsJson = "Not valid json format",
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.CreateSObject(input, options, _cancellationToken);
    }
}

