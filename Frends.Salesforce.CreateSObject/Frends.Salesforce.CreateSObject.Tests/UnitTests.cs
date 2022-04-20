using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

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

        private class JsonTest { 
            private string Name { get; }
            private DateTime Date { get; }

            public JsonTest(string name)
            {
                this.Name = name;
                this.Date = DateTime.Now;
            }
        }

        [SetUp]
        public void SetUp() {
            var temp = new JsonTest("Test");
            _userJson = JsonConvert.SerializeObject(temp);
        }

        [Test]
        public async Task CreateSObject()
        {
            var input = new Input
            {
                Domain = _domain,
                SObjectAsJson = _userJson
            };

            var options = new Options
            {
                AuthenticationMethod = AuthenticationMethod.AccessToken,
                AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
            };

            var result = await Salesforce.CreateSObject(input, options, _cancellationToken);
            Assert.IsTrue(result.RequestIsSuccessful);
        }

    }
}
