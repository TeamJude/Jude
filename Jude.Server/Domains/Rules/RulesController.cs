using System.Security.Claims;
using Jude.Server.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jude.Server.Domains.Rules;


[ApiController]
[Route("api/[controller]")]
public class RulesController : ControllerBase
{
    private readonly IRulesService _rulesService;

    public RulesController(IRulesService rulesService)
    {
        _rulesService = rulesService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateRule([FromBody] CreateRuleRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized("User not authenticated");

        var result = await _rulesService.CreateRule(request, userId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetRules([FromQuery] GetRulesRequest request)
    {
        var result = await _rulesService.GetRules(request);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }
}



