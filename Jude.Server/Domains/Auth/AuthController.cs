using System.Security.Claims;
using Jude.Server.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jude.Server.Domains.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.Register(request);

        if (!result.Success)
            return BadRequest(result.Errors);

        SetAuthCookie(HttpContext, result.Data!.Token);
        return Ok(result.Data.UserData);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.Login(request);

        if (!result.Success)
            return BadRequest(result.Errors);

        SetAuthCookie(HttpContext, result.Data!.Token);
        return Ok(result.Data.UserData);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetUserData()
    {
        if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            var result = await _authService.GetUserData(userId);
            return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
        }
        return Unauthorized("User not authenticated");
    }

    private static void SetAuthCookie(HttpContext httpContext, string token)
    {
        httpContext.Response.Cookies.Append(
            Constants.AccessTokenCookieName,
            token,
            new CookieOptions
            {
                HttpOnly = true,
                //TODO: set this back to true when we start using HTTPS on deployment
                Secure = false,
                SameSite = SameSiteMode.Lax,
                //set to 14days for
                Expires = DateTimeOffset.UtcNow.AddDays(14),
            }
        );
    }
}
