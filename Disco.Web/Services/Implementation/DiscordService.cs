using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Disco.Web.Exceptions.Discord;
using Disco.Web.Models.User;

namespace Disco.Web.Services;

public class DiscordService : IDiscordService
{
    private string _clientId { get; }
    private string _secret { get; }
    private string _redirectUrl { get; }
    private HttpClient _client { get; }

    public DiscordService(string clientId, string secret, string redirectUrl)
    {
        _clientId = clientId;
        _secret = secret;
        _redirectUrl = redirectUrl;
        _client = new();
    }

    public async Task<string> GetAuthorizationUrl(string state)
    {
        var str =
            "https://discord.com/api/oauth2/authorize?client_id="+UrlEncode(_clientId)+"&redirect_uri="+UrlEncode(_redirectUrl)+"&response_type=code&scope=identify&state=" + UrlEncode(state);
        return str;
    }

    public string GetDiscoRedirectUrl(string state)
    {
        return _redirectUrl;
    }

    private static string UrlEncode(string url)
    {
        return System.Web.HttpUtility.UrlEncode(url);
    }

    private async Task RevokeDiscordToken(string token)
    {
        const string revokeUrl = "https://discord.com/api/oauth2/token/revoke";
        try
        {
            var request = new Dictionary<string, string>()
            {
                { "client_id", _clientId },
                { "client_secret", _secret },
                { "token", token },
            };
            var response = await _client.PostAsync(revokeUrl, new FormUrlEncodedContent(request));
            if (response.StatusCode != HttpStatusCode.OK)
            {
                // TODO: logger
                throw new DiscordApiBadStatusException(response.StatusCode, await response.Content.ReadAsStringAsync(),
                    response.Headers);
            }
            // OK

        }
        catch (Exception e)
        {
            // TODO: logger
            Console.WriteLine("Error revoking discord token: {0}\n{1}",e.Message,e.StackTrace);
        }
    }

    private async Task<DiscordOauthResponse> RedeemDiscordCode(string code, string redirectUrl)
    {
        const string url = "https://discord.com/api/oauth2/token";
        var request = new Dictionary<string, string>()
        {
            { "client_id", _clientId },
            { "client_secret", _secret },
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUrl },
        };
        var response = await _client.PostAsync(url, new FormUrlEncodedContent(request));
        var responseString = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.OK)
            throw new DiscordApiBadStatusException(response.StatusCode, responseString, response.Headers);

        var newToken = JsonSerializer.Deserialize<DiscordOauthResponse>(responseString);
        if (newToken == null)
            throw new DiscordApiBadBodyException("Null body", response.StatusCode, responseString,
                response.Headers);
        if (string.IsNullOrWhiteSpace(newToken.accessToken))
            throw new DiscordApiBadBodyException("No access token", response.StatusCode, responseString,
                response.Headers);
        return newToken;
    }

    private async Task<DiscordUser> GetUser(string accessToken)
    {
        const string userUrl = "https://discord.com/api/v10/users/@me";
        // Get user info
        var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, userUrl);
        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        var response = await _client.SendAsync(userInfoRequest);
        var responseString = await response.Content.ReadAsStringAsync();

        if (response.StatusCode != HttpStatusCode.OK)
            throw new DiscordApiBadStatusException(response.StatusCode, responseString, response.Headers);
        
        var userInfo = JsonSerializer.Deserialize<DiscordUser>(responseString);
        if (userInfo == null)
            throw new DiscordApiBadBodyException("Null body", response.StatusCode, responseString,
                response.Headers);
        if (userInfo.bot)
            throw new DiscordApiBadBodyException("Bot account", response.StatusCode, responseString,
                response.Headers);
        if (userInfo.id == ""  || string.IsNullOrWhiteSpace(userInfo.discriminator) || string.IsNullOrWhiteSpace(userInfo.username))
            throw new DiscordApiBadBodyException("Missing fields: discrim="+userInfo.discriminator+ " id="+userInfo.id + " username=" + userInfo.username, response.StatusCode, responseString,
                response.Headers);

        return userInfo;
    }

    public async Task<DiscordUser> RedeemToken(string code, string redirectUrl)
    {
        var newToken = await RedeemDiscordCode(code, redirectUrl);
        try
        {
            var userInfo = await GetUser(newToken.accessToken);
            return userInfo;
        }
        finally
        {
            // Try to revoke. Don't really care if it fails.
            await RevokeDiscordToken(newToken.accessToken);
        }
    }
}