using Disco.Web.Models.User;

namespace Disco.Web.Services;

public interface IDiscordService
{
    Task<string> GetAuthorizationUrl(string state);
    string GetDiscoRedirectUrl(string state);
    Task<DiscordUser> RedeemToken(string code, string redirectUrl);
}