using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using dotenv.net;
using Frends.Salesforce.CreateSObject.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using JsonSerializer = System.Text.Json.JsonSerializer;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;

namespace Frends.Salesforce.CreateSObject.Tests;

[TestClass]
public class UnitTests
{
    private static string _clientSecret;
    private static string _password;
    private static string _securityToken;
    private static string _clientID;
    private static string _username;
    private static string _domain;

    private readonly CancellationToken _cancellationToken = new();
    private Connection _connection;
    private string _userJson;
    private List<object> _result;

    private string _name = "Test" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" +
                           DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Millisecond;

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
        DotEnv.Load();
        _clientSecret = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_SECRET");
        _password = Environment.GetEnvironmentVariable("SALESFORCE_PASSWORD");
        _securityToken = Environment.GetEnvironmentVariable("SALESFORCE_SECURITY_TOKEN");
        _clientID = Environment.GetEnvironmentVariable("SALESFORCE_CLIENTID");
        _username = Environment.GetEnvironmentVariable("SALESFORCE_USERNAME");
        _domain = Environment.GetEnvironmentVariable("SALESFORCE_DOMAIN_URL");
    }

    [TestInitialize]
    public async Task TestInitialize()
    {
        _result = new List<object>();

        _userJson = JsonSerializer.Serialize(new
        {
            Name = _name
        });

        _connection = new Connection
        {
            InstanceUrl = _domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await TestHelper.GetAccessToken(_domain, _clientID, _clientSecret),
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


                var client = new HttpClient()
                {
                    BaseAddress = new Uri(_domain)
                };
                var request = new HttpRequestMessage(HttpMethod.Delete,
                    "/services/data/v54.0/sobjects/" + obj.Type + "/" + obj.Id);
                request.Headers.Add("Authorization", "Bearer " + _connection.AccessToken);

                await client.SendAsync(request, _cancellationToken);
            }

            _result = null;
        }
    }

    [TestMethod]
    public async Task CreateAccountTest()
    {
        var input = new Input
        {
            SObjectAsJson = _userJson,
            SObjectType = "Account",
        };

        var result = await Salesforce.CreateSObject(input, _connection, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);

        var body = JsonConvert.SerializeObject(result.Body);
        var obj = JsonConvert.DeserializeObject<dynamic>(body);
        _result.Add(new
        {
            Type = "Account",
            Id = obj.id
        });
    }

    [TestMethod]
    public async Task CreateAccountTest_WithoutSpecifiedApiVersion()
    {
        var input = new Input
        {
            SObjectAsJson = _userJson,
            SObjectType = "Account"
        };

        var result = await Salesforce.CreateSObject(input, _connection, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful, result.ErrorMessage);

        var body = JsonConvert.SerializeObject(result.Body);
        var obj = JsonConvert.DeserializeObject<dynamic>(body);
        _result.Add(new
        {
            Type = "Account",
            Id = obj.id
        });
    }

    [TestMethod]
    public async Task CreateContactTest()
    {
        var json = JsonSerializer.Serialize(
            new
            {
                Title = "Mr",
                LastName = _name
            });

        var input = new Input
        {
            SObjectAsJson = json,
            SObjectType = "Contact",
        };

        var result = await Salesforce.CreateSObject(input, _connection, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);

        var body = JsonConvert.SerializeObject(result.Body);
        var obj = JsonConvert.DeserializeObject<dynamic>(body);
        _result.Add(new
        {
            Type = "Contact",
            Id = obj.id
        });
    }

    [TestMethod]
    public async Task CreateCaseTest()
    {
        // Creating an account to which case can be linked to.
        var accountInput = new Input
        {
            SObjectAsJson = _userJson,
            SObjectType = "Account"
        };

        var accountResult = await Salesforce.CreateSObject(accountInput, _connection, _cancellationToken);

        var body = JsonConvert.SerializeObject(accountResult.Body);
        var accObj = JsonConvert.DeserializeObject<dynamic>(body);
        _result.Add(new
        {
            Type = "Account",
            Id = accObj.id
        });

        // Creating a case.
        var json = JsonSerializer.Serialize(new
        {
            AccountId = accObj.id.ToString(),
            Subject = "This is a test.",
            Description = "This is a test case for Frends.SalesForce.CreateSObject task.",
            Origin = "Web"
        });

        var caseInput = new Input
        {
            SObjectAsJson = json,
            SObjectType = "Case"
        };

        var caseResult = await Salesforce.CreateSObject(caseInput, _connection, _cancellationToken);
        Assert.IsTrue(caseResult.RequestIsSuccessful);

        var caseBody = JsonConvert.SerializeObject(caseResult.Body);
        var caseObj = JsonConvert.DeserializeObject<dynamic>(caseBody);
        _result.Add(new
        {
            Type = "Case",
            Id = caseObj.id
        });
    }

    [TestMethod]
    public async Task GetReturnedAccessTokenTest()
    {
        var input = new Input
        {
            SObjectAsJson = _userJson,
            SObjectType = "Account"
        };

        var connection = new Connection
        {
            InstanceUrl = _domain,
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            ClientId = _clientID,
            ClientSecret = _clientSecret,
            Username = _username,
            Password = _password,
            SecurityToken = _securityToken,
            ReturnAccessToken = true
        };
        var result = await Salesforce.CreateSObject(input, connection, _cancellationToken);
        Assert.IsNotNull(result.Token);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task EmptyAccessToken_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = _userJson,
            SObjectType = "Contact"
        };

        var connection = new Connection()
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = " "
        };

        await Salesforce.CreateSObject(input, connection, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task EmptyDomain_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = _userJson,
            SObjectType = "Account"
        };

        var connection = new Connection()
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await TestHelper.GetAccessToken(_domain, _clientID, _clientSecret)
        };

        await Salesforce.CreateSObject(input, connection, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task EmptyJson_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = null,
            SObjectType = "Account"
        };

        var connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await TestHelper.GetAccessToken(_domain, _clientID, _clientSecret)
        };

        await Salesforce.CreateSObject(input, connection, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task EmptyType_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = _userJson,
            SObjectType = ""
        };

        var connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await TestHelper.GetAccessToken(_domain, _clientID, _clientSecret)
        };

        await Salesforce.CreateSObject(input, connection, _cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task InvalidDomain_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = _userJson,
            SObjectType = "Account"
        };

        var options = new Connection()
        {
            InstanceUrl = "https://mycompany.my.salesforce.com",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            ClientId = _clientID,
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
            SObjectAsJson = _userJson,
            SObjectType = "InvalidType"
        };

        var connection = new Connection
        {
            InstanceUrl = _domain,
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            ClientId = _clientID,
            ClientSecret = _clientSecret,
            Username = _username,
            Password = _password,
            SecurityToken = _securityToken
        };

        var result = await Salesforce.CreateSObject(input, connection, _cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code NotFound").ToString(),
            result.ErrorException.ToString());
    }

    [TestMethod]
    public async Task InvalidSecretOAuth_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = _userJson,
            SObjectType = "Account"
        };

        var connection = new Connection
        {
            InstanceUrl = _domain,
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            ClientId = _clientID,
            ClientSecret = "abcdefghijklmn123456789",
        };

        var ex = await Assert.ThrowsExactlyAsync<Exception>(async () =>
            await Salesforce.CreateSObject(input, connection, _cancellationToken));

        Assert.IsTrue(ex.Message.Contains("Failed to obtain access token"));
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task InvalidJson_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = "Not valid json format",
            SObjectType = "Account"
        };

        var connection = new Connection()
        {
            InstanceUrl = _domain,
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await TestHelper.GetAccessToken(_domain, _clientID, _clientSecret)
        };

        await Salesforce.CreateSObject(input, connection, _cancellationToken);
    }

    [TestMethod]
    public async Task CreateSObject_WithClientCredentials()
    {
        var input = new Input
        {
            SObjectAsJson = _userJson,
            SObjectType = "Account",
        };

        var connection = new Connection
        {
            InstanceUrl = _domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            ClientId = _clientID,
            ClientSecret = _clientSecret
        };

        var result = await Salesforce.CreateSObject(input, connection, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task CreateSObject_WithClientCredentials_ReturnToken()
    {
        var input = new Input
        {
            SObjectAsJson = _userJson,
            SObjectType = "Account",
        };

        var connection = new Connection()
        {
            InstanceUrl = _domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            ClientId = _clientID,
            ClientSecret = _clientSecret,
            ReturnAccessToken = true
        };

        var result = await Salesforce.CreateSObject(input, connection, _cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
        Assert.IsNotEmpty(result.Token);
    }
}
