namespace Jude.Server.Data.Models;

public class ClaimDataModel
{
    public Guid Id { get; set; }
    public int ClaimNumber { get; set; }
    public int TransactionNumber { get; set; }
    public int MembershipNumber { get; set; }
    public string PatientFirstName { get; set; }
    public string PatientLastName { get; set; }
    public int ProviderPracticeNumber { get; set; }
    public string ProviderPracticeName { get; set; }
    public string Currency { get; set; }
    public decimal ClaimAmount { get; set; }
    public decimal ClaimApprovedAmount { get; set; }
    public object Payload { get; set; }
}
