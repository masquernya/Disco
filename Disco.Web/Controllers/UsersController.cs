using System.ComponentModel.DataAnnotations;
using Disco.Web.Authentication;
using Disco.Web.Data;
using Disco.Web.Exceptions.Captcha;
using Disco.Web.Exceptions.RateLimit;
using Disco.Web.Exceptions.User;
using Disco.Web.Models.User;
using Disco.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Disco.Web.Controllers;

[ApiController]
[Route("/api/user")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private IUserService userService { get; }
    private IDiscordService discordService { get; }
    private IRateLimitService rateLimitService { get; }
    private ICaptchaService captchaService { get; }

    public UsersController(ILogger<UsersController> logger, IUserService userService, IDiscordService discordService, IRateLimitService rateLimitService, ICaptchaService captchaService)
    {
        _logger = logger;
        this.userService = userService;
        this.discordService = discordService;
        this.rateLimitService = rateLimitService;
        this.captchaService = captchaService;
    }

    private async Task CreateSession(long accountId)
    {
        _logger.Log(LogLevel.Information, "Creating a session for {accountId}",accountId);
        var auth = new SessionAuthentication(HttpContext, userService);
        await auth.CreateSession(accountId);
    }

    /// <summary>
    /// Create an account. On success, the user is logged in.
    /// </summary>
    /// <param name="request">The SignUpRequest model</param>
    /// <returns>The created user</returns>
    [HttpPost("SignUp")]
    public async Task<SignUpResponse> SignUp([Required, FromBody] SignUpRequest request)
    {
        await RateLimitIpOnly(30);
        if (string.IsNullOrWhiteSpace(request.captcha) || !await captchaService.IsValid(request.captcha))
            throw new CaptchaFailedException();
        
        var result = await userService.CreateAccount(request.username, request.password);
        await CreateSession(result.accountId);
        return new()
        {
            username = result.username,
            id = result.accountId,
        };
    }

    /// <summary>
    /// Set the authenticated users avatar source. If null, the avatar is deleted.
    /// </summary>
    /// <param name="request">The request</param>
    [HttpPost("SetAvatar")]
    public async Task SetAvatar([Required, FromBody] SetAvatarRequest request)
    {
        var session = await GetSession();
        await userService.SetAvatarSource(session.accountId, request.source);
    }

    [HttpPost("Login")]
    public async Task Login([Required, FromBody] LoginRequest request)
    {
        await RateLimitIpOnly(120);
        var account = await userService.GetAccountByName(request.username);
        if (account == null)
            throw new InvalidUsernameOrPasswordException();
        var passwordOk = await userService.IsPasswordValid(account.accountId, request.password);
        if (!passwordOk)
            throw new InvalidUsernameOrPasswordException();
        var banned = await userService.GetBan(account.accountId);
        if (banned != null)
            throw new AccountBannedException(banned.reason);
        // Create session
        await CreateSession(account.accountId);
    }

    [HttpGet("MyUser")]
    public async Task<MyAccountResponse> GetCurrentUser()
    {
        var sess = new SessionAuthentication(HttpContext, userService);
        var result = await sess.GetSession();
        if (result != null)
        {
            var item = await userService.GetAccountById(result.accountId);
            if (item != null)
            {
                var tags = await userService.GetTags(result.accountId);
                var desc = await userService.GetDescription(result.accountId);
                return new()
                {
                    accountId = item.accountId,
                    displayName = item.displayName,
                    username = item.username,
                    pronouns = item.pronouns,
                    gender = item.gender,
                    age = item.age,
                    description = desc?.description,
                    tags = tags.Select(c => new MyTag()
                    {
                        tag = c.displayTag,
                        tagId = c.accountTagId,
                    }),
                };
            }
        }
        throw new UnauthorizedException();
    }

    [HttpGet("Discord")]
    public async Task<AccountDiscord?> GetMyDiscord()
    {
        var sess = await GetSession();
        var discord = await userService.GetDiscordForAccount(sess.accountId);
        return discord;
    }

    [HttpGet("{accountId}/Discord")]
    public async Task<AccountDiscord?> GetOtherDiscord(long accountId)
    {
        var sess = await GetSession();
        var state = await userService.IsRelationshipMutual(sess.accountId, accountId);
        if (!state)
            throw new UnauthorizedException();
        
        var currentUserHasDiscord = await userService.GetDiscordForAccount(sess.accountId);
        if (currentUserHasDiscord == null)
            return null;
        
        var discord = await userService.GetDiscordForAccount(accountId);
        return discord;
    }

    [HttpDelete("Discord")]
    public async Task DeleteDiscord()
    {
        var sess = await GetSession();
        await userService.DeleteDiscord(sess.accountId);
    }
    
    
    [HttpGet("Matrix")]
    public async Task<AccountMatrix?> GetMyMatrix()
    {
        var sess = await GetSession();
        var matrix = await userService.GetMatrixForAccount(sess.accountId);
        return matrix;
    }

    [HttpPost("Matrix")]
    public async Task SetMatrix([Required, FromBody] SetMatrixAccountResponse request)
    {
        await RateLimitIpOnly(60);
        var sess = await GetSession();
        await userService.SetMatrixAccount(sess.accountId, request.username);
    }
    
    [HttpDelete("Matrix")]
    public async Task DeleteMyMatrix()
    {
        var sess = await GetSession();
        await userService.DeleteMatrix(sess.accountId);
    }
    
    [HttpGet("{accountId}/Matrix")]
    public async Task<AccountMatrix?> GetOtherMatrix(long accountId)
    {
        var sess = await GetSession();
        var state = await userService.IsRelationshipMutual(sess.accountId, accountId);
        if (!state)
            throw new UnauthorizedException();
        
        var currentUserHasMatrix = await userService.GetMatrixForAccount(sess.accountId);
        if (currentUserHasMatrix == null)
            return null;
        
        return await userService.GetMatrixForAccount(accountId);
    }

    [HttpGet("Avatar")]
    public async Task<AccountAvatar?> GetMyAvatar()
    {
        var sess = await GetSession();
        var avatar = await userService.GetAvatarForAccount(sess.accountId);
        return avatar;
    }

    [HttpDelete("Avatar")]
    public async Task DeleteMyAvatar()
    {
        var sess = await GetSession();
        await userService.DeleteAvatar(sess.accountId);
    }

    private async Task<AccountSession> GetSession()
    {
        var sess = new SessionAuthentication(HttpContext, userService);
        var result = await sess.GetSession();
        if (result == null)
            throw new UnauthorizedException();
        return result;
    }

    [HttpPut("Tag")]
    public async Task<AccountTag> AddTag([Required, FromBody] AddTagRequest request)
    {
        var sess = await GetSession();
        return await userService.AddTag(sess.accountId, request.tag);
    }

    [HttpDelete("Tag")]
    public async Task DeleteTagRequest([Required, FromBody] RemoveTagRequest request)
    {
        var sess = await GetSession();
        await userService.DeleteTag(sess.accountId, request.tagId);
    }

    [HttpPost("Description")]
    public async Task SetDescription([Required, FromBody] SetDescriptionRequest request)
    {
        var sess = await GetSession();
        await userService.SetDescription(sess.accountId, request.description);
    }

    [HttpPost("DisplayName")]
    public async Task SetDisplayName([Required, FromBody] SetDisplayNameRequest request)
    {
        var sess = await GetSession();
        await userService.SetDisplayName(sess.accountId, request.displayName);
    }

    [HttpPost("Gender")]
    public async Task SetGender([Required, FromBody] SetGenderRequest request)
    {
        var ses = await GetSession();
        await userService.SetGender(ses.accountId, request.gender);
    }

    [HttpPost("Pronouns")]
    public async Task SetPronouns([Required, FromBody] SetPronounsRequest request)
    {
        var sess = await GetSession();
        await userService.SetPronouns(sess.accountId, request.pronouns);
    }

    [HttpPost("Age")]
    public async Task SetAge([Required, FromBody] SetAgeRequest request)
    {
        var sess = await GetSession();
        await userService.SetAge(sess.accountId, request.age);
    }

    [HttpPost("UpdateRelationship")]
    public async Task<AccountRelationshipUpdateResponse> UpdateRelationship([Required, FromBody] UpdateRelationshipRequest request)
    {
        var sess = await GetSession();
       return await userService.UpdateRelationship(sess.accountId, request.targetAccountId, request.status);
    }

    [HttpGet("TopTags")]
    public async Task<IEnumerable<TopTagWithCount>> GetTopTags()
    {
        return (await userService.GetTopTags());
    }

    [HttpGet("FetchPotentialFriendsV1")]
    public async Task<FetchUsersResponse> FetchPotentialFriends()
    {
        var sess = await GetSession();
        return await userService.FetchUsers(sess.accountId);
    }
    
    [HttpGet("FetchMatches")]
    public async Task<FetchUsersResponse> FetchMatches(long startId = 0)
    {
        var sess = await GetSession();
        return await userService.FetchUsersByStatus(sess.accountId, RelationStatus.Accepted, startId);
    }

    [HttpPost("DiscordLinkUrl")]
    public async Task<LinkDiscordUrlResponse> LinkDiscordUrl()
    {
        var sess = await GetSession();
        var token = await userService.CreateDiscordToken(sess.accountId);
        var url = await discordService.GetAuthorizationUrl(token);
        await userService.SetDiscordTokenUrl(token, discordService.GetDiscoRedirectUrl(token));
        return new()
        {
            redirectUrl = url,
        };
    }

    [HttpGet("DiscordRedirect")]
    public async Task<IActionResult> HandleDiscordOauthResponse(string state, string code)
    {
        var sess = await GetSession();
        await RateLimitApiIpAndAccountId(sess.accountId, 60);
        var canRedeem = await userService.RedeemDiscordToken(state, sess.accountId);
        if (canRedeem == null)
            throw new Exception("This token is no longer redeemable");
        var discordInfo = await discordService.RedeemToken(code, canRedeem.redirectUrl);
        _logger.LogInformation("User is logged into discord as {username}#{discriminator}", discordInfo.username, discordInfo.discriminator);
        
        await userService.AttachDiscordAccount(sess.accountId, long.Parse(discordInfo.id),
            discordInfo.username + "#" + discordInfo.discriminator, discordInfo.avatar);
        
        return new RedirectResult(Config.frontendUrl+"/me?discordSuccess=true");
    }

    private string GetIp()
    {
        // TODO: this should configurable or something. if you aren't using cloudflare, this can be exploited.
        if (HttpContext.Request.Headers.TryGetValue("cf-connecting-ip", out var header))
            return header!;
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "UNKNOWN";
    }

    private async Task RateLimitApiIpAndAccountId(long userId, int maxPerHour)
    {
        var resource = HttpContext.Request.Path;
        _logger.LogInformation("RateLimitApiIpAndAccountId path is {resource}", resource);
        if (!await rateLimitService.TryIncrementResource(resource + ":userId:" + userId, maxPerHour, TimeSpan.FromHours(1)))
            throw new RateLimitReachedException();
        if (!await rateLimitService.TryIncrementResource(resource + ":ip:" + GetIp(), maxPerHour, TimeSpan.FromHours(1)))
            throw new RateLimitReachedException();
    }
    
    private async Task RateLimitIpOnly(int maxPerHour)
    {
        var resource = HttpContext.Request.Path;
        _logger.LogInformation("RateLimitIpOnly Path is {resource}", resource);
        if (!await rateLimitService.TryIncrementResource(resource + ":ip:" + GetIp(), maxPerHour, TimeSpan.FromHours(1)))
            throw new RateLimitReachedException();
    }
    
    [HttpPost("Password")]
    public async Task UpdateMyPassword([Required, FromBody] UpdatePasswordRequest request)
    {
        var sess = await GetSession();
        await RateLimitApiIpAndAccountId(sess.accountId, 10);
        await userService.UpdatePassword(sess.accountId, request.originalPassword, request.newPassword);
    }

    [HttpPost("Report")]
    public async Task ReportUser([Required, FromBody] ReportUserRequest request)
    {
        var sess = await GetSession();
        try
        {
            await RateLimitApiIpAndAccountId(sess.accountId, 120);
        }
        catch (RateLimitReachedException)
        {
            return;
        }
        await userService.ReportUser(sess.accountId, request.accountId, request.reason, request.field);
    }

    [HttpGet("PendingReports")]
    public async Task<IEnumerable<AccountReport>> GetPendingReportsAdmin()
    {
        var sess = await GetSession();
        if (!Config.IsAdmin(sess.accountId))
            throw new UnauthorizedException();
        return await userService.GetPendingReports();
    }
    
    [HttpPost("ResolveReport")]
    public async Task ResolveReportAdmin(long reportId)
    {
        var sess = await GetSession();
        if (!Config.IsAdmin(sess.accountId))
            throw new UnauthorizedException();
        await userService.SetReportState(reportId, ReportState.Resolved);
    }
    
    [HttpPost("BanAccount")]
    public async Task BanAccountAdmin([Required, FromBody] BanAccountRequest request)
    {
        var sess = await GetSession();
        if (!Config.IsAdmin(sess.accountId) || Config.IsAdmin(request.accountId))
            throw new UnauthorizedException();
        await userService.BanAccount(request.accountId, request.reason);
    }
        
    [HttpPost("UnbanAccount")]
    public async Task UnBanAccountAdmin([Required, FromBody] BanAccountRequest request)
    {
        var sess = await GetSession();
        if (!Config.IsAdmin(sess.accountId))
            throw new UnauthorizedException();
        await userService.UnbanAccount(request.accountId);
    }

    [HttpPost("RejectReport")]
    public async Task RejectReportAdmin(long reportId)
    {
        var sess = await GetSession();
        if (!Config.IsAdmin(sess.accountId))
            throw new UnauthorizedException();
        await userService.SetReportState(reportId, ReportState.Rejected);
    }

    [HttpGet("FullUserInfo")]
    public async Task<FullUserInfoResponse> GetFullUserInfoAdmin(long accountId)
    {
        var sess = await GetSession();
        if (!Config.IsAdmin(sess.accountId))
            throw new UnauthorizedException();
        var account = await userService.GetAccountById(accountId);
        if (account == null)
            throw new UserNotFoundException();
        
        var tags = await userService.GetTags(accountId);
        var desc = await userService.GetDescription(accountId);
        var discord = await userService.GetDiscordForAccount(accountId);
        var image = await userService.GetAvatarForAccount(accountId);
        var matrix = await userService.GetMatrixForAccount(accountId);
        return new(account)
        {
            accountTags = tags,
            description = desc,
            discord = discord,
            avatar = image,
            matrix = matrix,
        };
    }
    
    [HttpGet("FetchAllUsers")]
    public async Task<IEnumerable<Account>> FetchAllUsers()
    {
        var sess = await GetSession();
        if (!Config.IsAdmin(sess.accountId))
            throw new UnauthorizedException();
        return await userService.FetchAllUsers();
    }
    
    [HttpGet("FetchAllImagesAwaitingReview")]
    public async Task<IEnumerable<UserUploadedImageReview>> FetchAllImagesAwaitingReview()
    {
        var sess = await GetSession();
        if (!Config.IsAdmin(sess.accountId))
            throw new UnauthorizedException();
        return await userService.GetImagesAwaitingReview();
    }
    
    [HttpPost("ApproveImage")]
    public async Task ApproveImage([Required, FromBody] ToggleImageStatusRequest request)
    {
        var sess = await GetSession();
        if (!Config.IsAdmin(sess.accountId))
            throw new UnauthorizedException();
        await userService.SetImageStatus(request.imageId, ImageStatus.Approved);
    }
    
    [HttpPost("RejectImage")]
    public async Task RejectImage([Required, FromBody] ToggleImageStatusRequest request)
    {
        var sess = await GetSession();
        if (!Config.IsAdmin(sess.accountId))
            throw new UnauthorizedException();
        await userService.SetImageStatus(request.imageId, ImageStatus.Rejected);
        await userService.DeleteImage(request.imageId);
    }

    [HttpGet("FetchUnfilteredTopTags")]
    public async Task<IEnumerable<TopTagWithCount>> FetchUnfilteredTopTags()
    {
        var sess = await GetSession();
        if (!Config.IsAdmin(sess.accountId))
            throw new UnauthorizedException();

        return await userService.GetTopTagsUnfiltered();
    }
    
    [HttpPost("ApproveTopTag")]
    public async Task ApproveTopTag([Required, FromBody] ApproveTopTagRequest request)
    {
        var sess = await GetSession();
        if (!Config.IsAdmin(sess.accountId))
            throw new UnauthorizedException();
        await userService.ApproveTopTag(request.tag, request.displayTag);
    }
    
    [HttpPost("DeleteTopTag")]
    public async Task DeleteTopTag([Required, FromBody] DeleteTopTagRequest request)
    {
        var sess = await GetSession();
        if (!Config.IsAdmin(sess.accountId))
            throw new UnauthorizedException();
        await userService.DeleteTopTag(request.tag);
    }

    [HttpPost("DeleteAccount")]
    public async Task DeleteAccount()
    {
        await RateLimitIpOnly(15);
        var sess = await GetSession();
        await userService.DeleteAccount(sess.accountId);
    }

    [HttpPost("PasswordReset/Matrix")]
    public async Task<ResetPasswordResponse> CreateResetRequest([Required, FromBody] ResetPasswordWithMatrixRequest request)
    {
        await RateLimitIpOnly(15);
        var result = await userService.CreatePasswordResetRequestForMatrix(request.username, request.matrixUserId);
        
        return new()
        {
            token = result.token,
        };
    }

    [HttpPost("PasswordReset/Submit")]
    public async Task SubmitResetPasswordToken([Required, FromBody] ResetPasswordSubmitRequest request)
    {
        await RateLimitIpOnly(60);
        if (!await userService.TrySetPasswordFromToken(request.token, request.newPassword))
            throw new InvalidPasswordResetTokenException();
        // OK
    }
}