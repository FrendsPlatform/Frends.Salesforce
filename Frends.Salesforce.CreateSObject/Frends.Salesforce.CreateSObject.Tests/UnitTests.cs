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
    private static string clientSecret;
    private static string password;
    private static string securityToken;
    private static string clientID;
    private static string username;
    private static string domain;

    private readonly CancellationToken cancellationToken = CancellationToken.None;
    private Connection connection;
    private string userJson;
    private List<object> result;

    private string name = "Test" + DateTime.Now.Year + string.Empty + DateTime.Now.Month + string.Empty + DateTime.Now.Day + string.Empty +
                           DateTime.Now.Hour + string.Empty + DateTime.Now.Minute + string.Empty + DateTime.Now.Millisecond;

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
        DotEnv.Load();
        clientSecret = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_SECRET");
        password = Environment.GetEnvironmentVariable("SALESFORCE_PASSWORD");
        securityToken = Environment.GetEnvironmentVariable("SALESFORCE_SECURITY_TOKEN");
        clientID = Environment.GetEnvironmentVariable("SALESFORCE_CLIENTID");
        username = Environment.GetEnvironmentVariable("SALESFORCE_USERNAME");
        domain = Environment.GetEnvironmentVariable("SALESFORCE_DOMAIN_URL");
    }

    [TestInitialize]
    public async Task TestInitialize()
    {
        result = new List<object>();

        userJson = JsonSerializer.Serialize(new
        {
            Name = name,
        });

        connection = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await TestHelper.GetAccessToken(domain, clientID, clientSecret),
        };
    }

    [TestCleanup]
    public async Task TestCleanUp()
    {
        if (result != null)
        {
            for (var i = result.Count - 1; i >= 0; i--)
            {
                var temp = JsonConvert.SerializeObject(result[i]);
                var obj = JsonConvert.DeserializeObject<dynamic>(temp);

                var client = new HttpClient()
                {
                    BaseAddress = new Uri(domain),
                };
                var request = new HttpRequestMessage(
                    HttpMethod.Delete,
                    "/services/data/v54.0/sobjects/" + obj.Type + "/" + obj.Id);
                request.Headers.Add("Authorization", "Bearer " + connection.AccessToken);

                await client.SendAsync(request, cancellationToken);
            }

            result = null;
        }
    }

    [TestMethod]
    public async Task CreateAccountTest()
    {
        var input = new Input
        {
            SObjectAsJson = userJson,
            SObjectType = "Account",
        };

        var result = await Salesforce.CreateSObject(input, connection, new Options(), cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);

        var body = JsonConvert.SerializeObject(result.Body);
        var obj = JsonConvert.DeserializeObject<dynamic>(body);
        this.result.Add(new
        {
            Type = "Account",
            Id = obj.id,
        });
    }

    [TestMethod]
    public async Task CreateAccountTest_WithoutSpecifiedApiVersion()
    {
        var input = new Input
        {
            SObjectAsJson = userJson,
            SObjectType = "Account",
        };

        var result = await Salesforce.CreateSObject(input, connection, new Options(), cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful, result.ErrorMessage);

        var body = JsonConvert.SerializeObject(result.Body);
        var obj = JsonConvert.DeserializeObject<dynamic>(body);
        this.result.Add(new
        {
            Type = "Account",
            Id = obj.id,
        });
    }

    [TestMethod]
    public async Task CreateContactTest()
    {
        var json = JsonSerializer.Serialize(
            new
            {
                Title = "Mr",
                LastName = name,
            });

        var input = new Input
        {
            SObjectAsJson = json,
            SObjectType = "Contact",
        };

        var result = await Salesforce.CreateSObject(input, connection, new Options(), cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);

        var body = JsonConvert.SerializeObject(result.Body);
        var obj = JsonConvert.DeserializeObject<dynamic>(body);
        this.result.Add(new
        {
            Type = "Contact",
            Id = obj.id,
        });
    }

    [TestMethod]
    public async Task CreateCaseTest()
    {
        // Creating an account to which case can be linked to.
        var accountInput = new Input
        {
            SObjectAsJson = userJson,
            SObjectType = "Account",
        };

        var accountResult = await Salesforce.CreateSObject(accountInput, connection, new Options(), cancellationToken);

        var body = JsonConvert.SerializeObject(accountResult.Body);
        var accObj = JsonConvert.DeserializeObject<dynamic>(body);
        result.Add(new
        {
            Type = "Account",
            Id = accObj.id,
        });

        // Creating a case.
        var json = JsonSerializer.Serialize(new
        {
            AccountId = accObj.id.ToString(),
            Subject = "This is a test.",
            Description = "This is a test case for Frends.SalesForce.CreateSObject task.",
            Origin = "Web",
        });

        var caseInput = new Input
        {
            SObjectAsJson = json,
            SObjectType = "Case",
        };

        var caseResult = await Salesforce.CreateSObject(caseInput, connection, new Options(), cancellationToken);
        Assert.IsTrue(caseResult.RequestIsSuccessful);

        var caseBody = JsonConvert.SerializeObject(caseResult.Body);
        var caseObj = JsonConvert.DeserializeObject<dynamic>(caseBody);
        result.Add(new
        {
            Type = "Case",
            Id = caseObj.id,
        });
    }

    [TestMethod]
    public async Task GetReturnedAccessTokenTest()
    {
        var input = new Input
        {
            SObjectAsJson = userJson,
            SObjectType = "Account",
        };

        var connection = new Connection
        {
            InstanceUrl = domain,
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            ClientId = clientID,
            ClientSecret = clientSecret,
            Username = username,
            Password = password,
            SecurityToken = securityToken,
            ReturnAccessToken = true,
        };
        var result = await Salesforce.CreateSObject(input, connection, new Options(), cancellationToken);
        Assert.IsNotNull(result.Token);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task EmptyAccessToken_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = userJson,
            SObjectType = "Contact",
        };

        var connection = new Connection()
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = " ",
        };

        await Salesforce.CreateSObject(input, connection, new Options(), cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task EmptyDomain_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = userJson,
            SObjectType = "Account",
        };

        var connection = new Connection()
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await TestHelper.GetAccessToken(domain, clientID, clientSecret),
        };

        await Salesforce.CreateSObject(input, connection, new Options(), cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task EmptyJson_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = null,
            SObjectType = "Account",
        };

        var connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await TestHelper.GetAccessToken(domain, clientID, clientSecret),
        };

        await Salesforce.CreateSObject(input, connection, new Options(), cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task EmptyType_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = userJson,
            SObjectType = string.Empty,
        };

        var connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await TestHelper.GetAccessToken(domain, clientID, clientSecret),
        };

        await Salesforce.CreateSObject(input, connection, new Options(), cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task InvalidDomain_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = userJson,
            SObjectType = "Account",
        };

        var options = new Connection()
        {
            InstanceUrl = "https://mycompany.my.salesforce.com",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            ClientId = clientID,
            ClientSecret = clientSecret,
            Username = username,
            Password = password + securityToken,
        };

        await Salesforce.CreateSObject(input, options, new Options(), cancellationToken);
    }

    [TestMethod]
    public async Task InvalidObjectType_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = userJson,
            SObjectType = "InvalidType",
        };

        var connection = new Connection
        {
            InstanceUrl = domain,
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            ClientId = clientID,
            ClientSecret = clientSecret,
            Username = username,
            Password = password,
            SecurityToken = securityToken,
        };

        var result = await Salesforce.CreateSObject(
            input,
            connection,
            new Options { ThrowErrorOnFailure = false },
            cancellationToken);
        Assert.AreEqual(
            new HttpRequestException("Request failed with status code NotFound").ToString(),
            result.ErrorException.ToString());
    }

    [TestMethod]
    public async Task InvalidSecretOAuth_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = userJson,
            SObjectType = "Account",
        };

        var connection = new Connection
        {
            InstanceUrl = domain,
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            ClientId = clientID,
            ClientSecret = "abcdefghijklmn123456789",
        };

        var ex = await Assert.ThrowsExactlyAsync<Exception>(async () =>
            await Salesforce.CreateSObject(input, connection, new Options(), cancellationToken));

        Assert.IsTrue(ex.Message.Contains("Failed to obtain access token"));
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task InvalidJson_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = "Not valid json format",
            SObjectType = "Account",
        };

        var connection = new Connection()
        {
            InstanceUrl = domain,
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = await TestHelper.GetAccessToken(domain, clientID, clientSecret),
        };

        await Salesforce.CreateSObject(input, connection, new Options(), cancellationToken);
    }

    [TestMethod]
    public async Task CreateSObject_WithClientCredentials()
    {
        var input = new Input
        {
            SObjectAsJson = userJson,
            SObjectType = "Account",
        };

        var connection = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            ClientId = clientID,
            ClientSecret = clientSecret,
        };

        var result = await Salesforce.CreateSObject(input, connection, new Options(), cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task CreateSObject_WithClientCredentials_ReturnToken()
    {
        var input = new Input
        {
            SObjectAsJson = userJson,
            SObjectType = "Account",
        };

        var connection = new Connection()
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            ClientId = clientID,
            ClientSecret = clientSecret,
            ReturnAccessToken = true,
        };

        var result = await Salesforce.CreateSObject(input, connection, new Options(), cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
        Assert.IsNotEmpty(result.Token);
    }
}
