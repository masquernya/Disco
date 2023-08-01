using Disco.Web.Data;
using Disco.Web.Services;

namespace Disco.Web.Authentication;

public class SessionAuthentication
{
    private const string cookieName = "dauth";
    private HttpContext _ctx { get; }
    private IUserService _userService { get; }
    
    public SessionAuthentication(HttpContext ctx, IUserService userService)
    {
        this._ctx = ctx;
        _userService = userService;
    }

    public async Task CreateSession(long accountId)
    {
        var session = await _userService.CreateSession(accountId);
        _ctx.Response.Cookies.Append(cookieName, session.accountSessionId, new CookieOptions()
        {
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
#if RELEASE
            Secure = true,
#endif
            Path = "/",
            MaxAge = TimeSpan.FromDays(365),
        });
    }

    public async Task<AccountSession?> GetSession()
    {
        if (_ctx.Request.Cookies.TryGetValue(cookieName, out var cookieId))
        {
            if (!string.IsNullOrWhiteSpace(cookieId) && cookieId.Length > 64 && cookieId.Length < 1024)
            {
                return await _userService.GetSessionAndUpdate(cookieId);
                
            }
        }

        return null;
    }
}