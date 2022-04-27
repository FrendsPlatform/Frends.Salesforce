﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;
using static Frends.Salesforce.CreateSObject.Definitions.Enums;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Frends.Salesforce.CreateSObject.Tests
{
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
        private ResultObject _result;

        private string _name = "Test" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Millisecond;

        #region helper classes
        private class Account
        { 
            public string Name { get; set; }
        }

        private class Case
        {
            public string AccountId { get; set; }
            public string Status { get; set; } = "New";
            public string Origin { get; set; }
            public string Description { get; set; }
            public string Subject { get; set; }
        }

        private class ResultObject
        { 
            public SObjectType Type { get; set; }
            public string Id { get; set; }
        }
        #endregion

        [TestInitialize]
        public async Task TestInitialize()
        {
            Account content = new Account
            {
                Name = _name
            };
            _userJson = JsonSerializer.Serialize(content);

            _options = new Options
            {
                AuthenticationMethod = AuthenticationMethod.AccessToken,
                AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
            };
        }
#if false
        [TestCleanup]
        public async Task TestCleanUp()
        {
            if (_result != null)
            {
                var client = new RestClient(_domain + "/services/data/v54.0/sobjects/" + _result.Type + "/" + _result.Id);
                var request = new RestRequest("/", Method.Delete);

                request.AddHeader("Authorization", "Bearer " + _options.AccessToken);
                var response = await client.ExecuteAsync(request, _cancellationToken);
                _result = null;
            }
        }
        #endif

        [TestMethod]
        public async Task TestCreateSObjects()
        {
            await AssertAccount();
            await AssertCase();
        }

        #region Create record types

        private async Task AssertAccount() {
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

        private async Task AssertCase()
        {
            Case content = new Case
            {
                AccountId = _result.Id,
                Subject = "This is a test.",
                Description = "This is a test case for Frends.SalesForce.CreateSObject task.",
                Origin = "Web"
            };
            var json = JsonSerializer.Serialize(content);

            var input = new Input
            {
                Domain = _domain,
                SObjectAsJson = json,
                SObjectType = SObjectType.Case
            };

            var result = await Salesforce.CreateSObject(input, _options, _cancellationToken);
            Assert.IsTrue(result.RequestIsSuccessful);
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task TestThrow_EmptyAccessToken() {
            var input = new Input
            {
                Domain = _domain,
                SObjectAsJson = _userJson,
                SObjectType = SObjectType.Account
            };

            Options options = new Options
            {
                AuthenticationMethod = AuthenticationMethod.AccessToken,
                AccessToken = " "
            };

            var result = await Salesforce.CreateSObject(input, options, _cancellationToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestThrow_EmptyDomain()
        {
            var input = new Input
            {
                Domain = null,
                SObjectAsJson = _userJson,
                SObjectType = SObjectType.Account
            };

            Options options = new Options
            {
                AuthenticationMethod = AuthenticationMethod.AccessToken,
                AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
            };

            var result = await Salesforce.CreateSObject(input, options, _cancellationToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestThrow_EmptyJson()
        {
            var input = new Input
            {
                Domain = _domain,
                SObjectAsJson = null,
                SObjectType = SObjectType.Account
            };

            Options options = new Options
            {
                AuthenticationMethod = AuthenticationMethod.AccessToken,
                AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
            };

            var result = await Salesforce.CreateSObject(input, options, _cancellationToken);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task TestThrow_InvalidDomain()
        {
            var input = new Input
            {
                Domain = @"https://mycompany.my.salesforce.com",
                SObjectAsJson = _userJson,
                SObjectType = SObjectType.Account
            };

            Options options = new Options
            {
                AuthenticationMethod = AuthenticationMethod.AccessToken,
                AccessToken = await Salesforce.GetAccessToken(_authurl, _clientID, _clientSecret, _username, _password + _securityToken, _cancellationToken)
            };

            var result = await Salesforce.CreateSObject(input, options, _cancellationToken);
        }
    }
}
