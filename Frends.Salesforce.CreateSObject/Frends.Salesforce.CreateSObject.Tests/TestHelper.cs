using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Frends.Salesforce.CreateSObject.Tests;

public static class TestHelper
{
    internal static async Task<string> GetAccessToken(string url, string clientId, string clientSecret)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(url),
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/services/oauth2/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
        });
        using var response = await client.SendAsync(request).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        string accessToken = JsonConvert.DeserializeObject<dynamic>(responseContent).access_token;

        return accessToken;
    }
}
