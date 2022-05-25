using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Frends.Salesforce.ExecuteQuery.Tests
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

        [Test]
        public async Task ExecuteQuery_QueryWithToken()
        {
            var input = new Input
            {
                Domain = _domain,
                Query = "SELECT Name from Customer"
            };

            var options = new Options
            {
                AuthenticationMethod = AuthenticationMethod.AccessToken,
                AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
            };

            var result = await Salesforce.ExecuteQuery(input, options, _cancellationToken);
            Assert.IsTrue(result.RequestIsSuccessful);
        }

        [Test]
        public async Task ExecuteQuery_QueryWithPassword()
        {
            var input = new Input
            {
                Domain = _domain,
                Query = "SELECT Name from Customer"
            };

            var options = new Options
            {
                AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
                AuthUrl = _authurl,
                ClientID = _clientID,
                ClientSecret = _clientSecret,
                Username = _username,
                Password = _password,
                SecurityToken = _securityToken
            };

            var result = await Salesforce.ExecuteQuery(input, options, _cancellationToken);
            Assert.IsTrue(result.RequestIsSuccessful);
        }

        [Test]
        public async Task ExecuteQuery_QueryWithPassword_ReturnToken()
        {
            var input = new Input
            {
                Domain = _domain,
                Query = "SELECT Name from Customer"
            };

            var options = new Options
            {
                AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
                AuthUrl = _authurl,
                ClientID = _clientID,
                ClientSecret = _clientSecret,
                Username = _username,
                Password = _password,
                SecurityToken = _securityToken,
                ReturnAccessToken = true
            };

            var result = (ResultWithToken)await Salesforce.ExecuteQuery(input, options, _cancellationToken);
            var accessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken);
            Assert.IsTrue(result.RequestIsSuccessful);
            Assert.AreEqual(result.Token, accessToken);
        }
    }
}
