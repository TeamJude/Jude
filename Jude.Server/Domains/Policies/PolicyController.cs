using System.Security.Claims;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jude.Server.Domains.Policies;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PolicyController : ControllerBase
{
    private readonly IPolicyService _policyService;

    public PolicyController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadPolicy([FromForm] PolicyUploadRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        var result = await _policyService.UploadPolicyAsync(request.File, request.Name, userId);
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetPolicies([FromQuery] GetPoliciesRequest request)
    {
        var result = await _policyService.GetPolicies(request);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }
} 