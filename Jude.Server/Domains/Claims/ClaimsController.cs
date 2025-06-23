using System.Security.Claims;
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
    private readonly IClaimsService _claimsService;
    private readonly ICIMASProvider _cimasProvider;
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(
        IClaimsService claimsService,
        ICIMASProvider cimasProvider, 
        ILogger<ClaimsController> logger)
    {
        _claimsService = claimsService;
        _cimasProvider = cimasProvider;
        _logger = logger;
    }

    [HttpGet]
    [HasPermission(Features.Claims, Permission.Read)]
    public async Task<IActionResult> GetClaims([FromQuery] GetClaimsRequest request)
    {
        var result = await _claimsService.GetClaims(request);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpGet("{id}")]
    [HasPermission(Features.Claims, Permission.Read)]
    public async Task<IActionResult> GetClaim(Guid id)
    {
        var result = await _claimsService.GetClaim(id);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpPost("from-cimas")]
    [HasPermission(Features.Claims, Permission.Write)]
    public async Task<IActionResult> CreateClaimFromCIMAS([FromBody] CreateClaimFromCIMASRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized("User not authenticated");

        var result = await _claimsService.CreateClaimFromCIMAS(request, userId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpPut("{id}/status")]
    [HasPermission(Features.Claims, Permission.Write)]
    public async Task<IActionResult> UpdateClaimStatus(Guid id, [FromBody] UpdateClaimStatusRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized("User not authenticated");

        var result = await _claimsService.UpdateClaimStatus(id, request, userId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpPost("{id}/review")]
    [HasPermission(Features.Claims, Permission.Write)]
    public async Task<IActionResult> ReviewClaim(Guid id, [FromBody] ReviewClaimRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized("User not authenticated");

        var result = await _claimsService.ReviewClaim(id, request, userId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpPost("{id}/process")]
    [HasPermission(Features.Claims, Permission.Write)]
    public async Task<IActionResult> ProcessClaim(Guid id, [FromBody] ProcessClaimRequest request)
    {
        var result = await _claimsService.ProcessClaim(id, request);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpPost("batch-review")]
    [HasPermission(Features.Claims, Permission.Write)]
    public async Task<IActionResult> BatchReviewClaims([FromBody] BatchReviewClaimsRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized("User not authenticated");

        var result = await _claimsService.BatchReviewClaims(request, userId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpPost("resubmit")]
    [HasPermission(Features.Claims, Permission.Write)]
    public async Task<IActionResult> ResubmitClaim([FromBody] ResubmitClaimRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized("User not authenticated");

        var result = await _claimsService.ResubmitClaim(request, userId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
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
