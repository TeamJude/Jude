namespace Jude.Server.Data.Models;

public class CitationModel
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // "Policy" or "Tariff"
    public string Source { get; set; } = string.Empty; // Policy name or tariff code
    public string Quote { get; set; } = string.Empty; // Exact quote from policy or tariff details
    public string Context { get; set; } = string.Empty; // How this citation was used in decision
    public DateTime CitedAt { get; set; } = DateTime.UtcNow;

    public Guid ClaimId { get; set; }
    public ClaimModel Claim { get; set; } = null!;
}
