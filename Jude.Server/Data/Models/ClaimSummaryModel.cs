using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jude.Server.Data.Models;

public class ClaimSummaryModel
{
    public Guid Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
    public string PatientFirstName { get; set; } = string.Empty;
    public string PatientSurname { get; set; } = string.Empty;
    public string MedicalSchemeName { get; set; } = string.Empty;
    public decimal TotalClaimAmount { get; set; }
    
    [ForeignKey(nameof(ClaimModel))]
    public Guid ClaimId { get; set; }
    public ClaimModel Claim { get; set; } = null!;
} 