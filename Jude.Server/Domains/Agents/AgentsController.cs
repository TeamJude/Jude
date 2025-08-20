using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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

    [HttpPost("extract")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 104857600)] // 100 MB
    [RequestSizeLimit(104857600)]
    public async Task<IActionResult> Extract(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded or file is empty." });
        }

        var markdown = await _agentService.ExtractClaimDataAsync(file);
        return Ok(new { content = markdown });
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
