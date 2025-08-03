using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jude.Server.Domains.Audit;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpPost]
    public async Task<ActionResult<AuditResponse>> CreateAuditEntry([FromBody] CreateAuditRequest request)
    {
        var result = await _auditService.CreateAuditEntryAsync(request);
        return result.Match<ActionResult<AuditResponse>>(
            success => Ok(success),
            error => BadRequest(error)
        );
    }

    [HttpGet]
    public async Task<ActionResult<GetAuditResponse>> GetAuditEntries([FromQuery] GetAuditRequest request)
    {
        var result = await _auditService.GetAuditEntriesAsync(request);
        return result.Match<ActionResult<GetAuditResponse>>(
            success => Ok(success),
            error => BadRequest(error)
        );
    }

    [HttpGet("entity/{entityId}/{entityType}")]
    public async Task<ActionResult<AuditResponse[]>> GetEntityAuditHistory(Guid entityId, string entityType)
    {
        var result = await _auditService.GetEntityAuditHistoryAsync(entityId, entityType);
        return result.Match<ActionResult<AuditResponse[]>>(
            success => Ok(success),
            error => BadRequest(error)
        );
    }
}
