using System.Security.Claims;
using Jude.Server.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jude.Server.Domains.Fraud;

[ApiController]
[Route("api/[controller]")]
public class FraudController : ControllerBase
{
    private readonly IFraudService _fraudService;

    public FraudController(IFraudService fraudService)
    {
        _fraudService = fraudService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateFraudIndicator([FromBody] CreateFraudIndicatorRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized("User not authenticated");

        var result = await _fraudService.CreateFraudIndicator(request, userId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetFraudIndicators([FromQuery] GetFraudIndicatorsRequest request)
    {
        var result = await _fraudService.GetFraudIndicators(request);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }
}