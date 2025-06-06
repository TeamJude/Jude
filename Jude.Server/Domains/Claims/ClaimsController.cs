using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [HttpGet]
    [HasPermission("Claims", Permission.Read)]
    public async Task<IActionResult> GetClaims()
    {
        return Ok(new { message = "Claims list endpoint - requires Read permission" });
    }

    [HttpGet("{id}")]
    [HasPermission("Claims", Permission.Read)]
    public async Task<IActionResult> GetClaim(Guid id)
    {
        return Ok(new { message = $"Get claim {id} - requires Read permission" });
    }

    [HttpPost]
    [HasPermission("Claims", Permission.Write)]
    public async Task<IActionResult> CreateClaim()
    {
        return Ok(new { message = "Create claim endpoint - requires Write permission" });
    }

    [HttpPut("{id}")]
    [HasPermission("Claims", Permission.Write)]
    public async Task<IActionResult> UpdateClaim(Guid id)
    {
        return Ok(new { message = $"Update claim {id} - requires Write permission" });
    }

    [HttpDelete("{id}")]
    [HasPermission("Claims", Permission.Write)]
    public async Task<IActionResult> DeleteClaim(Guid id)
    {
        return Ok(new { message = $"Delete claim {id} - requires Write permission" });
    }
} 