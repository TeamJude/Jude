using System.Text.Json.Serialization;

namespace Jude.Server.Providers.CIMAS;

public record TokenPair(string AccessToken, string RefreshToken);

public record GetMemberInput(int MembershipNumber, int Suffix, string AccessToken);

public record Member
{
    [JsonPropertyName("Product")]
    public ProductInfo Product { get; set; } = new();

    [JsonPropertyName("Currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("MembershipNumber")]
    public int MembershipNumber { get; set; }

    [JsonPropertyName("Insurer")]
    public InsurerInfo Insurer { get; set; } = new();

    [JsonPropertyName("CellPhoneNo")]
    public string CellPhoneNo { get; set; } = string.Empty;

    [JsonPropertyName("Email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("DependantNo")]
    public int DependantNo { get; set; }

    [JsonPropertyName("DependantType")]
    public string DependantType { get; set; } = string.Empty;

    [JsonPropertyName("IdType")]
    public string IdType { get; set; } = string.Empty;

    [JsonPropertyName("IdNumber")]
    public int IdNumber { get; set; }

    [JsonPropertyName("FirstNames")]
    public string FirstNames { get; set; } = string.Empty;

    [JsonPropertyName("Initials")]
    public string Initials { get; set; } = string.Empty;

    [JsonPropertyName("Surname")]
    public string Surname { get; set; } = string.Empty;

    [JsonPropertyName("Birthdate")]
    public string Birthdate { get; set; } = string.Empty;

    [JsonPropertyName("Gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("JoinDate")]
    public string JoinDate { get; set; } = string.Empty;

    [JsonPropertyName("Status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("BenefitStartDate")]
    public string BenefitStartDate { get; set; } = string.Empty;

    [JsonPropertyName("ManagedCare")]
    public bool ManagedCare { get; set; }

    [JsonPropertyName("biometric_verification")]
    public bool BiometricVerification { get; set; }

    [JsonPropertyName("allow_bypass")]
    public bool AllowBypass { get; set; }

    [JsonPropertyName("has_fingerprint")]
    public bool HasFingerprint { get; set; }

    [JsonPropertyName("list_fpos")]
    public List<string> ListFpos { get; set; } = [];
}

public record ProductInfo
{
    [JsonPropertyName("Code")]
    public int Code { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;
}

public record InsurerInfo
{
    [JsonPropertyName("Code")]
    public int Code { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("FunderCode")]
    public string FunderCode { get; set; } = string.Empty;
}

public record GetPastClaimsInput(string PracticeNumber, string AccessToken);

public record Claim
{
    [JsonPropertyName("TransactionResponse")]
    public TransactionResponse TransactionResponse { get; set; } = new();

    [JsonPropertyName("Member")]
    public ClaimMember Member { get; set; } = new();

    [JsonPropertyName("Patient")]
    public Patient Patient { get; set; } = new();

    [JsonPropertyName("ClaimHeaderResponse")]
    public ClaimHeaderResponse ClaimHeaderResponse { get; set; } = new();

    [JsonPropertyName("ServiceResponse")]
    public object? ServiceResponse { get; set; }

    [JsonPropertyName("ProductResponse")]
    public List<ProductResponse> ProductResponse { get; set; } = [];
}

public record TransactionResponse
{
    [JsonPropertyName("Type")]
    public object? Type { get; set; }

    [JsonPropertyName("Number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("ClaimNumber")]
    public string ClaimNumber { get; set; } = string.Empty;

    [JsonPropertyName("DateTime")]
    public string DateTime { get; set; } = string.Empty;

    [JsonPropertyName("SubmittedBy")]
    public string SubmittedBy { get; set; } = string.Empty;

    [JsonPropertyName("Reversed")]
    public bool Reversed { get; set; }

    [JsonPropertyName("DateReversed")]
    public object? DateReversed { get; set; }
}

public record ClaimMember
{
    [JsonPropertyName("MedicalSchemeNumber")]
    public int MedicalSchemeNumber { get; set; }

    [JsonPropertyName("MedicalSchemeName")]
    public string MedicalSchemeName { get; set; } = string.Empty;

    [JsonPropertyName("Currency")]
    public string Currency { get; set; } = string.Empty;
}

public record Patient
{
    [JsonPropertyName("DependantCode")]
    public int DependantCode { get; set; }

    [JsonPropertyName("Personal")]
    public PersonalInfo Personal { get; set; } = new();
}

public record PersonalInfo
{
    [JsonPropertyName("Surname")]
    public string Surname { get; set; } = string.Empty;

    [JsonPropertyName("FirstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("Initials")]
    public object? Initials { get; set; }

    [JsonPropertyName("Gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("DateOfBirth")]
    public string DateOfBirth { get; set; } = string.Empty;
}

public record ClaimHeaderResponse
{
    [JsonPropertyName("ResponseCode")]
    public string ResponseCode { get; set; } = string.Empty;

    [JsonPropertyName("TotalValues")]
    public TotalValues TotalValues { get; set; } = new();
}

public record TotalValues
{
    [JsonPropertyName("Claimed")]
    public string Claimed { get; set; } = string.Empty;

    [JsonPropertyName("Copayment")]
    public string Copayment { get; set; } = string.Empty;

    [JsonPropertyName("SchemeAmount")]
    public string SchemeAmount { get; set; } = string.Empty;

    [JsonPropertyName("SavingsAmount")]
    public string SavingsAmount { get; set; } = string.Empty;

    [JsonPropertyName("NettMember")]
    public string NettMember { get; set; } = string.Empty;

    [JsonPropertyName("NettProvider")]
    public string NettProvider { get; set; } = string.Empty;
}

public record ProductResponse
{
    [JsonPropertyName("Number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("Code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("SubTotalValues")]
    public TotalValues SubTotalValues { get; set; } = new();

    [JsonPropertyName("Message")]
    public Message Message { get; set; } = new();

    [JsonPropertyName("TotalValues")]
    public TotalValues TotalValues { get; set; } = new();
}

public record Message
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Code")]
    public object? Code { get; set; }

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;
}

public record APIResponse<T>
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public T Data { get; set; } = default!;
}