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
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(IClaimsService claimsService, ILogger<ClaimsController> logger)
    {
        _claimsService = claimsService;
        _logger = logger;
    }

    [HttpGet("member/{membershipNumber}/{suffix}")]
    public async Task<IActionResult> GetMember(int membershipNumber, int suffix)
    {
        var result = await _claimsService.GetMemberAsync(membershipNumber, suffix);
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpGet("past-claims/{practiceNumber}")]
    public async Task<IActionResult> GetPastClaims(string practiceNumber)
    {
        var result = await _claimsService.GetPastClaimsAsync(practiceNumber);
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitClaim([FromBody] ClaimRequest request)
    {
        var result = await _claimsService.SubmitClaimAsync(request);
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpPost("reverse/{transactionNumber}")]
    public async Task<IActionResult> ReverseClaim(string transactionNumber)
    {
        var result = await _claimsService.ReverseClaimAsync(transactionNumber);
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok();
    }


}
