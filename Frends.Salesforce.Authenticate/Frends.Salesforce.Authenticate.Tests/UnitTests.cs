namespace Frends.Salesforce.Authenticate.Tests;

using dotenv.net;
using Frends.Salesforce.Authenticate.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Authentication;

[TestClass]
public class UnitTests
{
    private readonly string clientSecret = Environment.GetEnvironmentVariable("Salesforce_Client_Secret");
    private readonly string password = Environment.GetEnvironmentVariable("Salesforce_Password");
    private readonly string securityToken = Environment.GetEnvironmentVariable("Salesforce_Security_Token");
    private readonly string clientID = Environment.GetEnvironmentVariable("Salesforce_ClientID");
    private readonly string domain = @"https://frends2-dev-ed.develop.my.salesforce.com";
    private readonly string username = "jefim+tasks@frends.com";
    private readonly string authurl = @"https://login.salesforce.com/services/oauth2/token";

    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context)
    {
        var root = Directory.GetCurrentDirectory();
        string projDir = Directory.GetParent(root).Parent.Parent.FullName;
        DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { $"{projDir}/.env.local" }));
    }

    [TestMethod]
    public async Task Authenticate_ShouldReturnAccessToken_OnSuccess()
    {
        var input = new Input
        {
            LoginUrl = authurl,
            ClientId = clientID,
            ClientSecret = clientSecret,
            Username = username,
            Password = password,
            SecurityToken = securityToken,
        };

        var result = await Salesforce.Authenticate(input, CancellationToken.None);

        Assert.IsTrue(result.Success);
        Assert.IsFalse(string.IsNullOrEmpty(result.AccessToken));
    }

    [TestMethod]
    public async Task Authenticate_ThrowOnInvalidCredentials()
    {
        var input = new Input
        {
            LoginUrl = authurl,
            ClientId = "invalid_client_id",
            ClientSecret = "invalid_client_secret",
            Username = "invalid_username",
            Password = "invalid_password",
            SecurityToken = "invalid_security_token",
        };

        var exception = await Assert.ThrowsExceptionAsync<Exception>(
        () => Salesforce.Authenticate(input, CancellationToken.None));

        Assert.IsTrue(exception.Message.Contains("Bad Request"), "The exception message did not contain 'Bad request'.");
    }

    [TestMethod]
    public async Task Authenticate_ThrowWhenCredentialsAreNull()
    {
        Input input = null;
        await Assert.ThrowsExceptionAsync<NullReferenceException>(() => Salesforce.Authenticate(input, CancellationToken.None));
    }

    [TestMethod]
    public async Task AccessToken_ShouldBeValid()
    {
        var authParams = new Input
        {
            LoginUrl = authurl,
            ClientId = clientID,
            ClientSecret = clientSecret,
            Username = username,
            Password = password,
            SecurityToken = securityToken,
        };

        var accessToken = await Salesforce.GetAccessTokenAsync(authParams, CancellationToken.None);

        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await httpClient.GetAsync(domain + "/services/data/v54.0/");

            response.EnsureSuccessStatusCode();
        }
    }
}
