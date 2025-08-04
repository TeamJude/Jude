using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jude.Server.Domains.Audit;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] GetAuditLogsRequest request)
    {
        var result = await _auditService.GetAuditLogsAsync(request);
        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }
}
