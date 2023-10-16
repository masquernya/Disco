using System.ComponentModel.DataAnnotations;
using Disco.Web.Exceptions.Matrix;
using Disco.Web.Exceptions.User;
using Disco.Web.Models.Bot;
using Disco.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Disco.Web.Controllers;

[ApiController]
[Route("/api/Bot")]
public class BotController : ControllerBase
{
    public const string BotAuthorizationHeaderName = "BotAuthorization";
    private readonly ILogger _logger;
    private IUserService userService { get; }
    private IBotService botService { get; }
    private IHttpRequestService httpRequestService { get; }
    private IMatrixSpaceService spaceService {get;}

    public BotController(ILogger<BotController> logger, IUserService userService, IBotService botService, IHttpRequestService httpRequestService, IMatrixSpaceService spaceService)
    {
        this._logger = logger;
        this.userService = userService;
        this.botService = botService;
        this.httpRequestService = httpRequestService;
        this.spaceService = spaceService;
    }

    [NonAction]
    public bool IsAuthorized()
    {
        var providedValue = httpRequestService.GetRequestHeader(BotAuthorizationHeaderName);
        var expectedValue = botService.GetAuthorizationKey();
        if (providedValue != expectedValue)
            return false;
        return true;
    }

    [HttpPost("ResetPasswordMatrix")]
    public async Task<ResetPasswordVerificationResponse> ResetPasswordMatrix(
        [Required, FromBody] ResetPasswordVerification request)
    {
        if (!IsAuthorized())
            throw new UnauthorizedException();

        if (string.IsNullOrWhiteSpace(request.userId))
            throw new InvalidMatrixUsernameException();
        if (string.IsNullOrWhiteSpace(request.token))
            throw new Exception("Invalid token");

        var resetToken = await userService.TryRedeemMatrixPasswordResetRequest(request.userId, request.token);
        if (resetToken == null)
            throw new Exception("Invalid token");
        return new()
        {
            redirectUrl = Config.frontendUrl + "/reset-password/token/" + System.Web.HttpUtility.UrlEncode(resetToken),
        };
    }

    [HttpPost("AddOrUpdateSpace")]
    public async Task AddOrUpdateSpace([Required, FromBody] AddOrUpdateSpaceRequest request)
    {
        if (!IsAuthorized())
            throw new UnauthorizedException();
        
        if (string.IsNullOrWhiteSpace(request.invite))
            throw new ArgumentException("Null or empty", nameof(request.invite));
        
        if (request.memberCount < 0)
            throw new ArgumentException("Invalid member count", nameof(request.memberCount));
        
        if (string.IsNullOrWhiteSpace(request.name))
            throw new ArgumentException("Null or empty", nameof(request.name));
        
        await spaceService.AddOrUpdateMatrixSpace(request.invite, request.name, request.description, request.memberCount, request.avatar, request.is18Plus, request.admins);
    }
}