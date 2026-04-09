using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.UpdateSObject.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;
using dotenv.net;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Frends.Salesforce.UpdateSObject.Tests;

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

    private readonly string name = "Test" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" +
                                   DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Millisecond;

    private Connection connection;
    private string userJson;
    private List<object> resultList;

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
        resultList = new List<object>();
        userJson = JsonSerializer.Serialize(new
        {
            Name = name,
        });

        connection = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };
    }

    [TestCleanup]
    public async Task TestCleanUp()
    {
        if (resultList != null)
        {
            for (var i = (resultList.Count - 1); i >= 0; i--)
            {
                var temp = JsonConvert.SerializeObject(resultList[i]);
                var obj = JsonConvert.DeserializeObject<dynamic>(temp);

                var client = new RestClient(domain + "/services/data/v54.0/sobjects/" + obj.Type + "/" + obj.Id);
                var request = new RestRequest("/", Method.Delete);

                request.AddHeader("Authorization", "Bearer " + connection.AccessToken);
                await client.ExecuteAsync(request, cancellationToken);
            }

            resultList = null;
        }
    }

    [TestMethod]
    public async Task UpdateAccountTest()
    {
        var id = await CreateSObject("Account", userJson);
        resultList.Add(new
        {
            Type = "Account",
            Id = id,
        });

        var newInput = new
        {
            Name = "NewName_" + name,
        };
        var result = await Salesforce.UpdateSObject(new Input
        {
            SObjectId = id,
            SObjectType = "Account",
            SObjectAsJson = JsonSerializer.Serialize(newInput),
        }, connection, cancellationToken);

        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task UpdateAccountTest_WithoutSpecifiedApiVersion()
    {
        var con = new Connection
        {
            InstanceUrl = domain,
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };
        var id = await CreateSObject("Account", userJson);
        resultList.Add(new
        {
            Type = "Account",
            Id = id,
        });

        var newInput = new
        {
            Name = "NewName_" + name,
        };
        var result = await Salesforce.UpdateSObject(new Input
        {
            SObjectId = id,
            SObjectType = "Account",
            SObjectAsJson = JsonSerializer.Serialize(newInput),
        }, con, cancellationToken);

        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task UpdateContactTest()
    {
        var json = JsonSerializer.Serialize(
            new
            {
                Title = "Mr",
                LastName = name,
            });

        var id = await CreateSObject("Contact", json);
        resultList.Add(new
        {
            Type = "Contact",
            Id = id,
        });
        var result = await Salesforce.UpdateSObject(new Input
        {
            SObjectId = id,
            SObjectType = "Contact",
            SObjectAsJson = JsonSerializer.Serialize(
                new
                {
                    Title = "Mr",
                    LastName = "NewName_" + name,
                }
            ),
        }, connection, cancellationToken);

        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task UpdateCaseTest()
    {
        // Creating an account to which case can be linked to.
        var accountId = await CreateSObject("Account", userJson);
        resultList.Add(new
        {
            Type = "Account",
            Id = accountId,
        });

        // Creating a case.
        var json = JsonSerializer.Serialize(new
        {
            AccountId = accountId,
            Subject = "This is a test.",
            Description = "This is a test case for Frends.SalesForce.UpdateSObject task.",
            Origin = "Web",
        });

        var caseId = await CreateSObject("Case", json);
        resultList.Add(new
        {
            Type = "Case",
            Id = caseId,
        });

        var caseResult = await Salesforce.UpdateSObject(new Input
        {
            SObjectId = caseId,
            SObjectType = "Case",
            SObjectAsJson = JsonSerializer.Serialize(
                new
                {
                    AccountId = accountId,
                    Subject = "This is updated test.",
                    Description = "This is updated test case for Frends.SalesForce.UpdateSObject task.",
                    Origin = "Web",
                }
            ),
        }, connection, cancellationToken);
        Assert.IsTrue(caseResult.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task GetReturnedAccessTokenTest()
    {
        var id = await CreateSObject("Account", userJson);
        resultList.Add(new
        {
            Type = "Account",
            Id = id,
        });

        var newInput = new
        {
            Name = "NewName_" + name,
        };
        var con = new Connection
        {
            InstanceUrl = domain,
            SecurityToken = securityToken,
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Username = username,
            Password = password,
            ReturnAccessToken = true,
        };
        var result = await Salesforce.UpdateSObject(new Input
        {
            SObjectId = id,
            SObjectType = "Account",
            SObjectAsJson = JsonSerializer.Serialize(newInput),
        }, con, cancellationToken);

        Assert.IsNotNull(result.Token);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task EmptyAccessToken_ThrowTest()
    {
        var input = new Input
        {
            SObjectId = "123456789",
            SObjectAsJson = userJson,
            SObjectType = "Contact",
        };

        var con = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = " ",
        };

        await Salesforce.UpdateSObject(input, con, cancellationToken);
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
            InstanceUrl = null,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        await Salesforce.UpdateSObject(input, con, cancellationToken);
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
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        await Salesforce.UpdateSObject(input, con, cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task EmptyJson_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = null,
            SObjectType = "Account",
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        await Salesforce.UpdateSObject(input, con, cancellationToken);
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
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        await Salesforce.UpdateSObject(input, con, cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task InvalidDomain_ThrowTest()
    {
        var input = new Input
        {
            SObjectId = "123456789",
            SObjectAsJson = userJson,
            SObjectType = "Account",
        };

        var con = new Connection
        {
            InstanceUrl = "https://invaliddomain.salesforce.com",
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Username = username,
            Password = password,
        };

        await Salesforce.UpdateSObject(input, con, cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task InvalidObjectType_ThrowTest()
    {
        var input = new Input
        {
            SObjectId = "123456789",
            SObjectAsJson = userJson,
            SObjectType = "InvalidType",
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Username = username,
            Password = password,
        };

        var result = await Salesforce.UpdateSObject(input, con, cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code NotFound").ToString(),
            result.ErrorException.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task InvalidSecretOAuth_ThrowTest()
    {
        var input = new Input
        {
            SObjectId = "123456789",
            SObjectAsJson = userJson,
            SObjectType = "Account",
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            ClientId = clientId,
            ClientSecret = "abcdefghijklmn123456789",
            Username = username,
            Password = password,
        };

        var result = await Salesforce.UpdateSObject(input, con, cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code Unauthorized").ToString(),
            result.ErrorException.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task InvalidId_ThrowTest()
    {
        var input = new Input
        {
            SObjectId = "Not valid id",
            SObjectAsJson = userJson,
            SObjectType = "Account",
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        var result = await Salesforce.UpdateSObject(input, con, cancellationToken);
        Assert.AreEqual(new HttpRequestException("Request failed with status code NotFound").ToString(),
            result.ErrorException.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task InvalidJson_ThrowTest()
    {
        var input = new Input
        {
            SObjectAsJson = "Not valid json format",
            SObjectId = "123456789",
            SObjectType = "Account",
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
        };

        await Salesforce.UpdateSObject(input, con, cancellationToken);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task NotFoundId_ThrowTest()
    {
        var input = new Input
        {
            SObjectId = "123456789",
            SObjectType = "Account",
            SObjectAsJson = userJson,
        };

        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.AccessToken,
            AccessToken = token,
            ThrowAnErrorIfNotFound = true,
        };

        await Salesforce.UpdateSObject(input, con, cancellationToken);
    }


    [TestMethod]
    public async Task UpdateSObject_WithClientCredentials()
    {
        var id = await CreateSObject("Account", userJson);
        resultList.Add(new
        {
            Type = "Account",
            Id = id,
        });

        var newInput = new
        {
            Name = "NewName_" + name,
        };
        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            ClientId = clientId,
            ClientSecret = clientSecret,
        };

        var result = await Salesforce.UpdateSObject(new Input
        {
            SObjectId = id,
            SObjectType = "Account",
            SObjectAsJson = JsonSerializer.Serialize(newInput),
        }, con, cancellationToken);

        Assert.IsTrue(result.RequestIsSuccessful);
    }

    [TestMethod]
    public async Task UpdateSObject_WithClientCredentials_ReturnToken()
    {
        var id = await CreateSObject("Account", userJson);
        resultList.Add(new
        {
            Type = "Account",
            Id = id,
        });

        var newInput = new
        {
            Name = "NewName_" + name,
        };
        var con = new Connection
        {
            InstanceUrl = domain,
            ApiVersion = "v61.0",
            AuthenticationMethod = AuthenticationMethod.OAuth2WithClientCredentials,
            ClientId = clientId,
            ClientSecret = clientSecret,
            ReturnAccessToken = true,
        };

        var result = await Salesforce.UpdateSObject(new Input
        {
            SObjectId = id,
            SObjectType = "Account",
            SObjectAsJson = JsonSerializer.Serialize(newInput),
        }, con, cancellationToken);

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
