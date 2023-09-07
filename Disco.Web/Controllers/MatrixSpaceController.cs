using System.ComponentModel.DataAnnotations;
using Disco.Web.Authentication;
using Disco.Web.Data;
using Disco.Web.Exceptions.Captcha;
using Disco.Web.Exceptions.RateLimit;
using Disco.Web.Exceptions.User;
using Disco.Web.Models.Matrix;
using Disco.Web.Models.User;
using Disco.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Disco.Web.Controllers;

[ApiController]
[Route("/api/matrixspace")]
public class MatrixSpaceController : ControllerBase
{
    private readonly ILogger<MatrixSpaceController> _logger;
    private IMatrixSpaceService _matrixSpaceService;
    private IUserService userService { get; }

    public MatrixSpaceController(ILogger<MatrixSpaceController> logger, IMatrixSpaceService matrixSpaceService, IUserService userService)
    {
        _logger = logger;
        _matrixSpaceService = matrixSpaceService;
        this.userService = userService;
    } 
    
    private async Task<AccountSession> GetSession()
    {
        var sess = new SessionAuthentication(HttpContext, userService);
        var result = await sess.GetSession();
        if (result == null)
            throw new UnauthorizedException();
        return result;
    }
    
    [HttpGet("AllSpaces")]
    public async Task<IEnumerable<MatrixSpaceWithDetails>> GetAllSpaces()
    {
        return await _matrixSpaceService.GetAllSpaces();
    }
    
    [HttpGet("ManagedSpaces")]
    public async Task<IEnumerable<MatrixSpace>> GetManagedSpaces()
    {
        var session = await GetSession();
        return await _matrixSpaceService.GetManagedSpaces(session.accountId);
    }

    [HttpPost("Set18Plus")]
    public async Task SetIsMatrixSpace18Plus([Required, FromBody] SetIsMatrixSpace18PlusRequest request)
    {
        var session = await GetSession();
        await _matrixSpaceService.SetIs18Plus(session.accountId, request.matrixSpaceId, request.is18Plus);
    }

}