using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using NUnit.Framework;
using static Frends.Salesforce.CreateSObject.Definitions.Enums;
using System.Collections.Generic;

namespace Frends.Salesforce.CreateSObject.Tests
{
    [TestFixture]
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

        private string _userJson;

        #region helper classes
        private class JsonTest { 
            public string Name { get; set; }
        }
        #endregion

        [SetUp]
        public void SetUp() {
            JsonTest content = new JsonTest {
                Name = "Test" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day
            };
            var json = JsonSerializer.Serialize(content);
            _userJson = json;
        }

        [Test]
        public async Task CreateSObject()
        {
            var input = new Input
            {
                Domain = _domain,
                SObjectAsJson = _userJson,
                SObjectType = SObjectType.Account
            };

            var options = new Options
            {
                AuthenticationMethod = AuthenticationMethod.AccessToken,
                AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
            };

            Console.WriteLine(_userJson);

            var result = await Salesforce.CreateSObject(input, options, _cancellationToken);
            Assert.IsTrue(result.RequestIsSuccessful);
        }

    }
}
