﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.UpdateSObject.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Frends.Salesforce.UpdateSObject.Tests;

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
    private List<object> _result;

    readonly string _name = "Test" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Millisecond;

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
        var result = await Salesforce.UpdateSObject(new Input { Domain = _domain, SObjectId = id, SObjectType = "Account", SObjectAsJson = JsonSerializer.Serialize(newInput) }, _options, _cancellationToken);

        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task UpdateContactTest() {
        var json = JsonSerializer.Serialize(
            new
            {
                Title = "Mr",
                LastName = _name
            });

        var id = await CreateSObject("Contact", json);
        _result.Add(new { Type = "Contact", Id = id });
        var result = await Salesforce.UpdateSObject(new Input { Domain = _domain, SObjectId = id, SObjectType = "Contact", SObjectAsJson = JsonSerializer.Serialize(
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
        var json = JsonSerializer.Serialize(new {
            AccountId = accountId,
            Subject = "This is a test.",
            Description = "This is a test case for Frends.SalesForce.UpdateSObject task.",
            Origin = "Web"
        });

        var caseId = await CreateSObject("Case", json);
        _result.Add(new { Type = "Case", Id = caseId });

        var caseResult = await Salesforce.UpdateSObject(new Input { Domain = _domain, SObjectId = caseId, SObjectType = "Case", SObjectAsJson = JsonSerializer.Serialize(
                new
                {
                    AccountId = accountId,
                    Subject = "This is updated test.",
                    Description = "This is updated test case for Frends.SalesForce.UpdateSObject task.",
                    Origin = "Web"
                }
         )}, _options, _cancellationToken);
        Assert.IsTrue(caseResult.RequestIsSuccessful);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task EmptyAccessToken_ThrowTest() {
        var input = new Input
        {
            Domain = _domain,
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
    public async Task InvalidObjectType_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
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
    public async Task InvalidSecretOAuth_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
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
    public async Task InvalidId_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
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
    [ExpectedException(typeof(JsonException))]
    public async Task InvalidJson_ThrowTest()
    {
        var input = new Input
        {
            Domain = _domain,
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

