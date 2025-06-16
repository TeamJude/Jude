using System.Text.Json.Serialization;

namespace Jude.Server.Domains.Claims.Providers.CIMAS;

public record TokenPair(string AccessToken, string RefreshToken);

public record TokenResponse
{
    [JsonPropertyName("notify")]
    public object? Notify { get; set; }

    [JsonPropertyName("tokens")]
    public TokenPair Tokens { get; set; } = new(string.Empty, string.Empty);
}

public record GetMemberInput(int MembershipNumber, int Suffix, string AccessToken);

public record GetPastClaimsInput(string PracticeNumber, string AccessToken);

public record SubmitClaimInput(ClaimRequest Request, string AccessToken);

public record ReverseClaimInput(string TransactionNumber, string AccessToken);

public record UploadDocumentInput(string TransactionNumber, string Channel, string AccessToken, Stream FileStream, string FileName);

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
    public string IdNumber { get; set; } = string.Empty;

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

public record ClaimRequest
{
    [JsonPropertyName("Transaction")]
    public Transaction Transaction { get; set; } = new();

    [JsonPropertyName("Provider")]
    public Provider Provider { get; set; } = new();

    [JsonPropertyName("Member")]
    public ClaimMember Member { get; set; } = new();

    [JsonPropertyName("Patient")]
    public Patient Patient { get; set; } = new();

    [JsonPropertyName("ClaimHeader")]
    public ClaimHeader ClaimHeader { get; set; } = new();

    [JsonPropertyName("Practice")]
    public List<Practice> Practice { get; set; } = [];

    [JsonPropertyName("Products")]
    public List<Product> Products { get; set; } = [];
}

public record Transaction
{
    [JsonPropertyName("VersionNumber")]
    public string VersionNumber { get; set; } = "2.1";

    [JsonPropertyName("Type")]
    public string Type { get; set; } = "CL";

    [JsonPropertyName("DestinationCode")]
    public string DestinationCode { get; set; } = "CIMAS";

    [JsonPropertyName("SoftwareIdentifier")]
    public string SoftwareIdentifier { get; set; } = "CIMAS";

    [JsonPropertyName("DateTime")]
    public long DateTime { get; set; }

    [JsonPropertyName("TestClaimIndicator")]
    public string TestClaimIndicator { get; set; } = "Y";

    [JsonPropertyName("CountryISOCode")]
    public string CountryISOCode { get; set; } = "ZM";
}

public record Provider
{
    [JsonPropertyName("Role")]
    public string Role { get; set; } = "SP";

    [JsonPropertyName("PracticeNumber")]
    public string PracticeNumber { get; set; } = string.Empty;

    [JsonPropertyName("PracticeName")]
    public string PracticeName { get; set; } = string.Empty;
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

    [JsonPropertyName("NewBornIndicator")]
    public string NewBornIndicator { get; set; } = "N";

    [JsonPropertyName("Personal")]
    public PersonalInfo Personal { get; set; } = new();
}

public record PersonalInfo
{
    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("Surname")]
    public string Surname { get; set; } = string.Empty;

    [JsonPropertyName("FirstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("Initials")]
    public string Initials { get; set; } = string.Empty;

    [JsonPropertyName("Gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("IDNumber")]
    public string IDNumber { get; set; } = string.Empty;

    [JsonPropertyName("DateOfBirth")]
    public string DateOfBirth { get; set; } = string.Empty;
}

public record ClaimHeader
{
    [JsonPropertyName("ClaimNumber")]
    public string ClaimNumber { get; set; } = string.Empty;

    [JsonPropertyName("ClaimDateTime")]
    public long ClaimDateTime { get; set; }

    [JsonPropertyName("TotalServices")]
    public int TotalServices { get; set; }

    [JsonPropertyName("TotalProducts")]
    public int TotalProducts { get; set; }

    [JsonPropertyName("WhomToPay")]
    public string WhomToPay { get; set; } = "P";

    [JsonPropertyName("Outpatient")]
    public string Outpatient { get; set; } = "OutPatient";

    [JsonPropertyName("InHospitalIndicator")]
    public string InHospitalIndicator { get; set; } = "N";

    [JsonPropertyName("TotalValues")]
    public TotalValues TotalValues { get; set; } = new();
}

public record Practice
{
    [JsonPropertyName("PCNSNumber")]
    public string PCNSNumber { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;
}

public record Product
{
    [JsonPropertyName("ProductReference1")]
    public string ProductReference1 { get; set; } = string.Empty;

    [JsonPropertyName("Code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("System")]
    public string System { get; set; } = "NAPPI";

    [JsonPropertyName("Quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("SubTotalValues")]
    public TotalValues SubTotalValues { get; set; } = new();

    [JsonPropertyName("TotalValues")]
    public TotalValues TotalValues { get; set; } = new();
}

public record TotalValues
{
    [JsonPropertyName("GrossAmount")]
    public int GrossAmount { get; set; }

    [JsonPropertyName("NettAmount")]
    public int NettAmount { get; set; }

    [JsonPropertyName("PatientPayAmount")]
    public int PatientPayAmount { get; set; }
}

public record ClaimResponse
{
    [JsonPropertyName("TransactionResponse")]
    public TransactionResponse TransactionResponse { get; set; } = new();

    [JsonPropertyName("ClaimHeaderResponse")]
    public ClaimHeaderResponse ClaimHeaderResponse { get; set; } = new();

    [JsonPropertyName("ProductResponse")]
    public List<ProductResponse> ProductResponse { get; set; } = [];
}

public record TransactionResponse
{
    [JsonPropertyName("Type")]
    public string? Type { get; set; }

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
    public string? DateReversed { get; set; }
}

public record ClaimHeaderResponse
{
    [JsonPropertyName("ResponseCode")]
    public string ResponseCode { get; set; } = string.Empty;

    [JsonPropertyName("TotalValues")]
    public TotalValues TotalValues { get; set; } = new();
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
    public string? Code { get; set; }

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

public record RefreshTokenResponse
{
    [JsonPropertyName("access")]
    public string Access { get; set; } = string.Empty;
}

public record ClaimsResponse
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("next")]
    public object? Next { get; set; }

    [JsonPropertyName("previous")]
    public object? Previous { get; set; }

    [JsonPropertyName("results")]
    public List<ClaimWrapper> Results { get; set; } = [];
}

public record ClaimWrapper
{
    [JsonPropertyName("Response")]
    public ClaimResponse Response { get; set; } = new();
} 