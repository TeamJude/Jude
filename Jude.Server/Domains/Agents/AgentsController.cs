using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Jude.Server.Data.Repository;
using Jude.Server.Data.Models;

namespace Jude.Server.Domains.Agents;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly AgentService _agentService;
    private readonly ILogger<AgentsController> _logger;
    private readonly JudeDbContext _dbContext;

    public AgentsController(AgentService agentService, ILogger<AgentsController> logger, JudeDbContext dbContext)
    {
        _agentService = agentService;
        _logger = logger;
        _dbContext = dbContext;
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

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 104857600)] // 100 MB
    [RequestSizeLimit(104857600)]
    public async Task<IActionResult> UploadClaim(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded or file is empty." });
        }

        if (file.ContentType != "application/pdf")
        {
            return BadRequest(new { message = "Only PDF files are supported." });
        }

        try
        {
            // Process the uploaded claim
            var claimModel = await _agentService.ProcessUploadedClaimAsync(file);

            // Save to database
            _dbContext.Claims.Add(claimModel);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Successfully uploaded and processed claim {ClaimId}", claimModel.Id);

            return Ok(new { 
                success = true, 
                data = new {
                    id = claimModel.Id,
                    transactionNumber = claimModel.TransactionNumber,
                    patientFirstName = claimModel.PatientFirstName,
                    patientSurname = claimModel.PatientSurname,
                    medicalSchemeName = claimModel.MedicalSchemeName,
                    totalClaimAmount = claimModel.TotalClaimAmount,
                    status = claimModel.Status,
                    source = claimModel.Source,
                    ingestedAt = claimModel.IngestedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing uploaded claim");
            return StatusCode(500, new { message = "An error occurred while processing the uploaded claim." });
        }
    }
}
