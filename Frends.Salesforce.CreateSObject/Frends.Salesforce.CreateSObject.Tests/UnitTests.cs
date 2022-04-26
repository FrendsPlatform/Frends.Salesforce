using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RestSharp;
using static Frends.Salesforce.CreateSObject.Definitions.Enums;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
        private Options _options;

        private string _userJson;
        private ResultObject _result;

        #region helper classes
        private class InputObject { 
            public string Name { get; set; }
        }

        private class ResultObject { 
            public SObjectType Type { get; set; }
            public string Id { get; set; }
        }
        #endregion

        [SetUp]
        public async Task SetUp() {
            InputObject content = new InputObject {
                Name = "Test" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Millisecond
            };
            var json = JsonSerializer.Serialize(content);
            _userJson = json;

            _options = new Options
            {
                AuthenticationMethod = AuthenticationMethod.AccessToken,
                AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
            };
        }

        [Test]
        public async Task TestCreateSObject()
        {
            var input = new Input
            {
                Domain = _domain,
                SObjectAsJson = _userJson,
                SObjectType = SObjectType.Account
            };

            var result = await Salesforce.CreateSObject(input, _options, _cancellationToken);
            Assert.IsTrue(result.RequestIsSuccessful);

            _result = new ResultObject { Type = SObjectType.Account, Id = result.RecordId };
        }

        [TearDown]
        public async Task OneTimeTearDownAsync() {
            var client = new RestClient(_domain + "/services/data/v54.0/sobjects/" + _result.Type + "/" + _result.Id);
            var request = new RestRequest("/", Method.Delete);

            request.AddHeader("Authorization", "Bearer " + _options.AccessToken);
            var response = await client.ExecuteAsync(request, _cancellationToken);
            _result = null;
        }
    }
}
