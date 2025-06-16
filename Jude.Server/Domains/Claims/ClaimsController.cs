using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Domains.Auth.Authorization;
using Jude.Server.Domains.Claims.Providers.CIMAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jude.Server.Domains.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClaimsController : ControllerBase
{
    private readonly ICIMASProvider _cimasProvider;
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(ICIMASProvider cimasProvider, ILogger<ClaimsController> logger)
    {
        _cimasProvider = cimasProvider;
        _logger = logger;
    }

    [HttpGet]
    [HasPermission(Features.Claims, Permission.Read)]
    public async Task<IActionResult> GetClaims()
    {
        return Ok(new { message = "Claims list endpoint - requires Read permission" });
    }

    [HttpGet("{id}")]
    [HasPermission(Features.Claims, Permission.Read)]
    public async Task<IActionResult> GetClaim(Guid id)
    {
        return Ok(new { message = $"Get claim {id} - requires Read permission" });
    }

    [HttpPost]
    [HasPermission(Features.Claims, Permission.Write)]
    public async Task<IActionResult> CreateClaim()
    {
        return Ok(new { message = "Create claim endpoint - requires Write permission" });
    }

    [HttpPut("{id}")]
    [HasPermission(Features.Claims, Permission.Write)]
    public async Task<IActionResult> UpdateClaim(Guid id)
    {
        return Ok(new { message = $"Update claim {id} - requires Write permission" });
    }

    [HttpDelete("{id}")]
    [HasPermission(Features.Claims, Permission.Write)]
    public async Task<IActionResult> DeleteClaim(Guid id)
    {
        return Ok(new { message = $"Delete claim {id} - requires Write permission" });
    }

    #region CIMAS Provider Endpoints
    //these are just for testing purposes will probably use service directly to poll claims
    [HttpPost("cimas/token")]
    public async Task<IActionResult> GetCIMASToken()
    {
        var result = await _cimasProvider.GetAccessTokenAsync();
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpPost("cimas/token/refresh")]
    public async Task<IActionResult> RefreshCIMASToken([FromBody] string refreshToken)
    {
        var result = await _cimasProvider.RefreshAccessTokenAsync(refreshToken);
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpGet("cimas/member/{membershipNumber}/{suffix}")]
    public async Task<IActionResult> GetCIMASMember(int membershipNumber, int suffix, [FromHeader] string authorization)
    {
        var token = authorization.Replace("Bearer ", "");
        var result = await _cimasProvider.GetMemberAsync(new GetMemberInput(membershipNumber, suffix, token));
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpGet("cimas/past/{practiceNumber}")]
    public async Task<IActionResult> GetCIMASPastClaims(string practiceNumber, [FromHeader] string authorization)
    {
        var token = authorization.Replace("Bearer ", "");
        var result = await _cimasProvider.GetPastClaimsAsync(new GetPastClaimsInput(practiceNumber, token));
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpPost("cimas/submit")]
    public async Task<IActionResult> SubmitCIMASClaim([FromBody] SubmitClaimInput input, [FromHeader] string authorization)
    {
        var token = authorization.Replace("Bearer ", "");
        input = input with { AccessToken = token };
        var result = await _cimasProvider.SubmitClaimAsync(input);
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpPost("cimas/reverse")]
    public async Task<IActionResult> ReverseCIMASClaim([FromBody] ReverseClaimInput input, [FromHeader] string authorization)
    {
        var token = authorization.Replace("Bearer ", "");
        input = input with { AccessToken = token };
        var result = await _cimasProvider.ReverseClaimAsync(input);
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok();
    }

    [HttpPost("cimas/upload")]
    public async Task<IActionResult> UploadCIMASDocument([FromForm] UploadDocumentInput input, [FromHeader] string authorization)
    {
        var token = authorization.Replace("Bearer ", "");
        input = input with { AccessToken = token };
        var result = await _cimasProvider.UploadDocumentAsync(input);
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok();
    }

    #endregion
}
