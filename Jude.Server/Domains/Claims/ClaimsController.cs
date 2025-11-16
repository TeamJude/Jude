using System.Security.Claims;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Domains.Auth.Authorization;
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

    public ClaimsController(IClaimsService claimsService, ILogger<ClaimsController> _logger)
    {
        _claimsService = claimsService;
        this._logger = _logger;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats([FromQuery] ClaimsDashboardRequest request)
    {
        var result = await _claimsService.GetDashboardStatsAsync(request);
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetClaims([FromQuery] GetClaimsRequest request)
    {
        var result = await _claimsService.GetClaimsAsync(request);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }



    [HttpGet("{claimId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetClaim(Guid claimId)
    {
        var result = await _claimsService.GetClaimAsync(claimId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpPost("upload-excel")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
    [RequestSizeLimit(104857600)]
    [Authorize]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded or file is empty." });
        }

        var allowedExtensions = new[] { ".xlsx", ".xls" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest(new { message = "Only Excel files (.xlsx, .xls) are supported." });
        }

        using var stream = file.OpenReadStream();
        var result = await _claimsService.ProcessExcelUploadAsync(stream, file.FileName);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Errors.FirstOrDefault() });
        }

        return Ok(result.Data);
    }

    [HttpPost("{claimId:guid}/human-review")]
    [Authorize]
    public async Task<IActionResult> SubmitHumanReview(
        Guid claimId,
        [FromBody] SubmitHumanReviewRequest request
    )
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var result = await _claimsService.SubmitHumanReviewAsync(claimId, userGuid, request);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Errors.FirstOrDefault() });
        }

        return Ok(result.Data);
    }

    [HttpGet("{claimId:guid}/audit-logs")]
    [Authorize]
    public async Task<IActionResult> GetClaimAuditLogs(Guid claimId)
    {
        var result = await _claimsService.GetClaimAuditLogsAsync(claimId);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Errors.FirstOrDefault() });
        }

        return Ok(result.Data);
    }

}
