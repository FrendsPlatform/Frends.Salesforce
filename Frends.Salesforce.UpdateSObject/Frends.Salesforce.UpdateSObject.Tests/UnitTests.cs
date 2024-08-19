using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.UpdateSObject.Definitions;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;
using dotenv.net;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Frends.Salesforce.UpdateSObject.Tests;

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

    readonly string _name = "Test" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Millisecond;

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
        // load envs
        var root = Directory.GetCurrentDirectory();
        var projDir = Directory.GetParent(root)?.Parent?.Parent?.FullName;
        DotEnv.Load(
            options: new DotEnvOptions(envFilePaths: new[] { $"{projDir}{Path.DirectorySeparatorChar}.env.local" }));
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
            for (var i = (_result.Count - 1); i >= 0; i--)
            {
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
    public async Task UpdateAccountTest()
    {
        var id = await CreateSObject("Account", _userJson);
        _result.Add(new { Type = "Account", Id = id });

        var newInput = new { Name = "NewName_" + _name };
        var result = await Salesforce.UpdateSObject(new Input { Domain = _domain, ApiVersion = "v61.0", SObjectId = id, SObjectType = "Account", SObjectAsJson = JsonSerializer.Serialize(newInput) }, _options, _cancellationToken);

        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task UpdateAccountTest_WithoutSpecifiedApiVersion()
    {
        var id = await CreateSObject("Account", _userJson);
        _result.Add(new { Type = "Account", Id = id });

        var newInput = new { Name = "NewName_" + _name };
        var result = await Salesforce.UpdateSObject(new Input { Domain = _domain, SObjectId = id, SObjectType = "Account", SObjectAsJson = JsonSerializer.Serialize(newInput) }, _options, _cancellationToken);

        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task UpdateContactTest()
    {
        var json = JsonSerializer.Serialize(
            new
            {
                Title = "Mr",
                LastName = _name
            });

        var id = await CreateSObject("Contact", json);
        _result.Add(new { Type = "Contact", Id = id });
        var result = await Salesforce.UpdateSObject(new Input
        {
            Domain = _domain,
            ApiVersion = "v61.0",
            SObjectId = id,
            SObjectType = "Contact",
            SObjectAsJson = JsonSerializer.Serialize(
                new
                {
                    Title = "Mr",
                    LastName = "NewName_" + _name
                }
            )
        }, _options, _cancellationToken);

        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task UpdateCaseTest()
    {
        // Creating an account to which case can be linked to.
        var accountId = await CreateSObject("Account", _userJson);
        _result.Add(new { Type = "Account", Id = accountId });

        // Creating a case.
        var json = JsonSerializer.Serialize(new
        {
            AccountId = accountId,
            Subject = "This is a test.",
            Description = "This is a test case for Frends.SalesForce.UpdateSObject task.",
            Origin = "Web"
        });

        var caseId = await CreateSObject("Case", json);
        _result.Add(new { Type = "Case", Id = caseId });

        var caseResult = await Salesforce.UpdateSObject(new Input
        {
            Domain = _domain,
            ApiVersion = "v61.0",
            SObjectId = caseId,
            SObjectType = "Case",
            SObjectAsJson = JsonSerializer.Serialize(
                new
                {
                    AccountId = accountId,
                    Subject = "This is updated test.",
                    Description = "This is updated test case for Frends.SalesForce.UpdateSObject task.",
                    Origin = "Web"
                }
         )
        }, _options, _cancellationToken);
        Assert.IsTrue(caseResult.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task GetReturnedAccessTokenTest()
    {
        var id = await CreateSObject("Account", _userJson);
        _result.Add(new { Type = "Account", Id = id });

        var newInput = new { Name = "NewName_" + _name };
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
        var result = await Salesforce.UpdateSObject(new Input { Domain = _domain, ApiVersion = "v61.0", SObjectId = id, SObjectType = "Account", SObjectAsJson = JsonSerializer.Serialize(newInput) }, options, _cancellationToken);

        Assert.IsNotNull(result.Token);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task EmptyAccessToken_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            ApiVersion = "v61.0",
            SObjectId = "123456789",
            SObjectAsJson = _userJson,
            SObjectType = "Contact"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = " "
        };

        await Salesforce.UpdateSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task EmptyDomain_ThrowTest()
    {
        var input = new Input
        {
            Domain = null,
            ApiVersion = "v61.0",
            SObjectId = "123456789",
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.UpdateSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task EmptyId_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            ApiVersion = "v61.0",
            SObjectId = null,
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.UpdateSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task EmptyJson_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            ApiVersion = "v61.0",
            SObjectAsJson = null,
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.UpdateSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task EmptyType_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            ApiVersion = "v61.0",
            SObjectId = "123456789",
            SObjectType = ""
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.UpdateSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task InvalidDomain_ThrowTest()
    {
        var input = new Input
        {
            Domain = "https://mycompany.my.salesforce.com",
            ApiVersion = "v61.0",
            SObjectId = "123456789",
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

        await Salesforce.UpdateSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(RuntimeBinderException))]
    public async Task ExampleDomain_ThrowTest()
    {
        var input = new Input
        {
            Domain = "https://example.my.salesforce.com",
            ApiVersion = "v61.0",
            SObjectId = "123456789",
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

        await Salesforce.UpdateSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(RuntimeBinderException))]
    public async Task InvalidObjectType_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            ApiVersion = "v61.0",
            SObjectId = "123456789",
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

        var result = await Salesforce.UpdateSObject(input, options, _cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code NotFound").ToString(), result.ErrorException.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(RuntimeBinderException))]
    public async Task InvalidSecretOAuth_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            ApiVersion = "v61.0",
            SObjectId = "123456789",
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

        var result = await Salesforce.UpdateSObject(input, options, _cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code Unauthorized").ToString(), result.ErrorException.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(RuntimeBinderException))]
    public async Task InvalidId_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            ApiVersion = "v61.0",
            SObjectId = "Not valid id",
            SObjectAsJson = _userJson,
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        var result = await Salesforce.UpdateSObject(input, options, _cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code NotFound").ToString(), result.ErrorException.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(JsonReaderException))]
    public async Task InvalidJson_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            ApiVersion = "v61.0",
            SObjectAsJson = "Not valid json format",
            SObjectId = "123456789",
            SObjectType = "Account"
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
        };

        await Salesforce.UpdateSObject(input, options, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(HttpRequestException))]
    public async Task NotFoundId_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
            ApiVersion = "v61.0",
            SObjectId = "123456789",
            SObjectType = "Account",
            SObjectAsJson = _userJson
        };

        var options = new Options
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken),
            ThrowAnErrorIfNotFound = true
        };

        await Salesforce.UpdateSObject(input, options, _cancellationToken);
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

