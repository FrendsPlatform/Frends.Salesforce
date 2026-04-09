using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.DeleteSObject.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using dotenv.net;
using RestSharp;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Frends.Salesforce.DeleteSObject.Tests;

[TestClass]
public class UnitTests
{
    private static string clientSecret;
    private static string password;
    private static string securityToken;
    private static string clientId;
    private static string username;
    private static string domain;
    private static string token;

    private readonly CancellationToken cancellationToken = CancellationToken.None;
    private Connection connection;
    private string userJson;

    private readonly string name = "Test" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Millisecond;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext testContext)
    {
        DotEnv.Load();
        clientSecret = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_SECRET");
        password = Environment.GetEnvironmentVariable("SALESFORCE_PASSWORD");
        securityToken = Environment.GetEnvironmentVariable("SALESFORCE_SECURITY_TOKEN");
        clientId = Environment.GetEnvironmentVariable("SALESFORCE_CLIENTID");
        username = Environment.GetEnvironmentVariable("SALESFORCE_USERNAME");
        domain = Environment.GetEnvironmentVariable("SALESFORCE_DOMAIN_URL");

        token = await TestHelper.GetAccessToken(domain, clientId, clientSecret);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        userJson = JsonSerializer.Serialize(new { Name = name });

        connection = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };
    }

    [TestMethod]
    public async Task DeleteAccountTest()
    {
        var id = await CreateSObject("Account", userJson);
        var result = await Salesforce.DeleteSObject(new Input { SObjectId = id, SObjectType = "Account" }, connection, cancellationToken);

        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task DeleteAccountTest_WithoutSpecifiedApiVersion()
    {
        var id = await CreateSObject("Account", userJson);
        var con = new Connection
        {
            InstanceUrl = domain,
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };
        var result = await Salesforce.DeleteSObject(new Input { SObjectId = id, SObjectType = "Account" }, con, cancellationToken);

        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task DeleteContactTest()
    {
        var json = JsonSerializer.Serialize(
            new
            {
                Title = "Mr",
                LastName = name,
            });

        var id = await CreateSObject("Contact", json);
        var result = await Salesforce.DeleteSObject(new Input { SObjectId = id, SObjectType = "Contact" }, connection, cancellationToken);

        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task DeleteCaseTest()
    {
        // Creating an account to which case can be linked to.
        var accountId = await CreateSObject("Account", userJson);

        // Creating a case.
        var json = JsonSerializer.Serialize(new
        {
            AccountId = accountId,
            Subject = "This is a test.",
            Description = "This is a test case for Frends.SalesForce.DeleteSObject task.",
            Origin = "Web",
        });

        var caseId = await CreateSObject("Case", json);

        var caseResult = await Salesforce.DeleteSObject(new Input { SObjectId = caseId, SObjectType = "Case" }, connection, cancellationToken);
        Assert.IsTrue(caseResult.RequestIsSuccessful);

        var accountResult = await Salesforce.DeleteSObject(new Input { SObjectId = accountId, SObjectType = "Account" }, connection, cancellationToken);
        Assert.IsTrue(accountResult.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task GetReturnedAccessTokenTest()
    {
        var id = await CreateSObject("Account", userJson);
        var con = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            InstanceUrl = domain,
            SecurityToken = securityToken,

            ClientId = clientId,
            ClientSecret = clientSecret,
            Username = username,
            Password = password,
            ReturnAccessToken = true,
        };
        var result = await Salesforce.DeleteSObject(new Input { SObjectId = id, SObjectType = "Account" }, con, cancellationToken);

        Assert.IsNotNull(result.Token);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task EmptyAccessToken_ThrowTest()
    {
        var input = new Input
        {
            SObjectId = "123456789",
            SObjectType = "Contact",
        };

        var con = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = " ",
        };

        await Salesforce.DeleteSObject(input, con, cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task EmptyDomain_ThrowTest()
    {
        var input = new Input
        {
            SObjectId = "123456789",
            SObjectType = "Account",
        };

        var con = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        await Salesforce.DeleteSObject(input, con, cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task EmptyId_ThrowTest()
    {
        var input = new Input
        {
            SObjectId = null,
            SObjectType = "Account",
        };

        var con = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        await Salesforce.DeleteSObject(input, con, cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task EmptyType_ThrowTest()
    {
        var input = new Input
        {

            SObjectId = "123456789",
            SObjectType = "",
        };

        var con = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        await Salesforce.DeleteSObject(input, con, cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task InvalidDomain_ThrowTest()
    {
        var input = new Input
        {
            SObjectId = "123456789",
            SObjectType = "Account",
        };

        var con = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            InstanceUrl = "https://invaliddomain.my.salesforce.com",

            ClientId = clientId,
            ClientSecret = clientSecret,
            Username = username,
            Password = password + securityToken,
        };

        await Salesforce.DeleteSObject(input, con, cancellationToken);
    }

    [TestMethod]
    public async Task InvalidObjectType_ThrowTest()
    {
        var input = new Input
        {

            SObjectId = "123456789",
            SObjectType = "InvalidType",
        };

        var con = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            InstanceUrl = domain,
            SecurityToken = securityToken,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Username = username,
            Password = password,
        };

        var result = await Salesforce.DeleteSObject(input, con, cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code NotFound").ToString(), result.ErrorException.ToString());
    }

    [TestMethod]
    public async Task InvalidSecretOAuth_ThrowTest()
    {
        var input = new Input
        {

            SObjectId = "123456789",
            SObjectType = "Account",
        };

        var con = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            InstanceUrl = domain,
            SecurityToken = securityToken,
            ClientId = clientId,
            ClientSecret = "abcdefghijklmn123456789",
            Username = username,
            Password = password,
        };

        var ex = await Assert.ThrowsExactlyAsync<Exception>(async () =>
            await Salesforce.DeleteSObject(input, con, cancellationToken));

        Assert.IsTrue(ex.Message.Contains("Failed to obtain access token"));
    }

    [TestMethod]
    public async Task InvalidId_ThrowTest()
    {
        var input = new Input
        {

            SObjectId = "Not valid id",
            SObjectType = "Account",
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        var result = await Salesforce.DeleteSObject(input, con, cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code NotFound").ToString(), result.ErrorException.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task NotFoundId_ThrowTest()
    {
        var input = new Input
        {

            SObjectId = "123456789",
            SObjectType = "Account",
        };

        var con = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
            ThrowAnErrorIfNotFound = true,
        };

        await Salesforce.DeleteSObject(input, con, cancellationToken);
    }

    [TestMethod]
    public async Task DeleteSObject_WithClientCredentials()
    {
        var id = await CreateSObject("Account", userJson);
        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            ClientId = clientId,
            ClientSecret = clientSecret,
        };
        var result = await Salesforce.DeleteSObject(new Input { SObjectId = id, SObjectType = "Account" }, con, cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task DeleteSObject_WithClientCredentials_ReturnToken()
    {
        var id = await CreateSObject("Account", userJson);
        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            ClientId = clientId,
            ClientSecret = clientSecret,
            ReturnAccessToken = true,
        };
        var result = await Salesforce.DeleteSObject(new Input { SObjectId = id, SObjectType = "Account" }, con, cancellationToken);
        Assert.IsTrue(result.RequestIsSuccessful);
        Assert.IsNotEmpty(result.Token);
    }

    // Helper method to create SObjects for delete function.
    private async Task<string> CreateSObject(string type, string input)
    {
        var client = new RestClient(domain + "/services/data/v54.0/sobjects/" + type);
        var request = new RestRequest("/", Method.Post);

        var accessToken = token;
        request.AddHeader("Authorization", "Bearer " + accessToken);

        var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(input);
        request.RequestFormat = DataFormat.Json;
        request.AddJsonBody(json);

        var response = await client.ExecuteAsync(request, cancellationToken);
        var content = JsonConvert.DeserializeObject<dynamic>(response.Content);

        return content.id.ToString();
    }
}

