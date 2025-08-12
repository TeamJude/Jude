using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jude.Server.Domains.Agents;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly AgentService _agentService;
    private readonly ILogger<AgentsController> _logger;

    public AgentsController(AgentService agentService, ILogger<AgentsController> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Review([FromBody] string claimData)
    {
        var result = await _agentService.TestAgentAsync(claimData);
        if (result == null)
        {
            return BadRequest("Agent could not process the claim or returned invalid response.");
        }
        
        return Ok(result);
    }
}
