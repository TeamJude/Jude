using System.Text.Json.Serialization;

namespace Jude.Server.Domains.Claims.Providers.CIMAS;

public record TokenPair(string AccessToken, string RefreshToken);

public record CIMASTokens
{
    [JsonPropertyName("access")]
    public string Access { get; set; } = string.Empty;

    [JsonPropertyName("refresh")]
    public string? Refresh { get; set; }
}

public record TokenResponse
{
    [JsonPropertyName("notify")]
    public object? Notify { get; set; }

    [JsonPropertyName("tokens")]
    public CIMASTokens Tokens { get; set; } = new();
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

public record ClaimTotalValues
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

public record ClaimResponseMember
{
    [JsonPropertyName("MedicalSchemeNumber")]
    public int MedicalSchemeNumber { get; set; }

    [JsonPropertyName("MedicalSchemeName")]
    public string MedicalSchemeName { get; set; } = string.Empty;

    [JsonPropertyName("Currency")]
    public string Currency { get; set; } = string.Empty;
}

public record ClaimResponsePatient
{
    [JsonPropertyName("DependantCode")]
    public int DependantCode { get; set; }

    [JsonPropertyName("Personal")]
    public ClaimResponsePersonal Personal { get; set; } = new();
}

public record ClaimResponsePersonal
{
    [JsonPropertyName("Surname")]
    public string Surname { get; set; } = string.Empty;

    [JsonPropertyName("FirstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("Initials")]
    public string? Initials { get; set; }

    [JsonPropertyName("Gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("DateOfBirth")]
    public string DateOfBirth { get; set; } = string.Empty;
}

public record ServiceResponse
{
    [JsonPropertyName("Number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("Code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("SubTotalValues")]
    public ClaimTotalValues SubTotalValues { get; set; } = new();

    [JsonPropertyName("Message")]
    public Message Message { get; set; } = new();

    [JsonPropertyName("TotalValues")]
    public ClaimTotalValues TotalValues { get; set; } = new();
}

public record ClaimResponse
{
    [JsonPropertyName("TransactionResponse")]
    public TransactionResponse TransactionResponse { get; set; } = new();

    [JsonPropertyName("Member")]
    public ClaimResponseMember Member { get; set; } = new();

    [JsonPropertyName("Patient")]
    public ClaimResponsePatient Patient { get; set; } = new();

    [JsonPropertyName("ClaimHeaderResponse")]
    public ClaimHeaderResponse ClaimHeaderResponse { get; set; } = new();

    [JsonPropertyName("ServiceResponse")]
    public List<ServiceResponse> ServiceResponse { get; set; } = [];

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
    public ClaimTotalValues TotalValues { get; set; } = new();
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
    public ClaimTotalValues SubTotalValues { get; set; } = new();

    [JsonPropertyName("Message")]
    public Message Message { get; set; } = new();

    [JsonPropertyName("TotalValues")]
    public ClaimTotalValues TotalValues { get; set; } = new();
}

public record Message
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Code")]
    public string? Code { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }
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

// Pricing API Contracts
public record PricingApiTokenRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public record PricingApiTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
}

public record PricingApiErrorResponse
{
    [JsonPropertyName("errorMsg")]
    public string ErrorMsg { get; set; } = string.Empty;

    [JsonPropertyName("developerMsg")]
    public string DeveloperMsg { get; set; } = string.Empty;

    [JsonPropertyName("responseStatus")]
    public string ResponseStatus { get; set; } = string.Empty;

    [JsonPropertyName("responseCode")]
    public int ResponseCode { get; set; }
}

public record TariffLookupInput(string TariffCode, string PricingAccessToken);

// Dashboard Statistics Inputs
public record GetClaimStatsInput(string AccessToken, string? PracticeNumber = null, DateTime? FromDate = null, DateTime? ToDate = null);

public record GetMemberStatsInput(string AccessToken, string? PracticeNumber = null, DateTime? FromDate = null, DateTime? ToDate = null);

public record TariffPackage
{
    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public PackageName Name { get; set; } = new();

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}

public record PackageName
{
    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("package")]
    public string Package { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public record TariffCategory
{
    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}

public record TariffSubCategory
{
    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}

public record TariffResponse
{
    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("packages")]
    public List<TariffPackage> Packages { get; set; } = [];

    [JsonPropertyName("category")]
    public List<TariffCategory> Category { get; set; } = [];

    [JsonPropertyName("subCategory")]
    public List<TariffSubCategory> SubCategory { get; set; } = [];

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

// Dashboard Statistics Response Models
public record ClaimStats
{
    [JsonPropertyName("totalClaims")]
    public int TotalClaims { get; set; }

    [JsonPropertyName("totalClaimValue")]
    public decimal TotalClaimValue { get; set; }

    [JsonPropertyName("totalApprovedValue")]
    public decimal TotalApprovedValue { get; set; }

    [JsonPropertyName("averageClaimValue")]
    public decimal AverageClaimValue { get; set; }

    [JsonPropertyName("approvalRate")]
    public decimal ApprovalRate { get; set; }

    [JsonPropertyName("claimsByStatus")]
    public Dictionary<string, int> ClaimsByStatus { get; set; } = new();

    [JsonPropertyName("claimsByMonth")]
    public List<MonthlyClaimStats> ClaimsByMonth { get; set; } = [];

    [JsonPropertyName("topProviders")]
    public List<ProviderStats> TopProviders { get; set; } = [];
}

public record MemberStats
{
    [JsonPropertyName("totalMembers")]
    public int TotalMembers { get; set; }

    [JsonPropertyName("activeMembers")]
    public int ActiveMembers { get; set; }

    [JsonPropertyName("newMembersThisMonth")]
    public int NewMembersThisMonth { get; set; }

    [JsonPropertyName("membersByProduct")]
    public Dictionary<string, int> MembersByProduct { get; set; } = new();

    [JsonPropertyName("membersByGender")]
    public Dictionary<string, int> MembersByGender { get; set; } = new();

    [JsonPropertyName("averageAge")]
    public decimal AverageAge { get; set; }

    [JsonPropertyName("membershipGrowth")]
    public List<MonthlyMemberStats> MembershipGrowth { get; set; } = [];
}

public record MonthlyClaimStats
{
    [JsonPropertyName("month")]
    public string Month { get; set; } = string.Empty;

    [JsonPropertyName("claimCount")]
    public int ClaimCount { get; set; }

    [JsonPropertyName("totalValue")]
    public decimal TotalValue { get; set; }
}

public record ProviderStats
{
    [JsonPropertyName("providerName")]
    public string ProviderName { get; set; } = string.Empty;

    [JsonPropertyName("claimCount")]
    public int ClaimCount { get; set; }

    [JsonPropertyName("totalValue")]
    public decimal TotalValue { get; set; }
}

public record MonthlyMemberStats
{
    [JsonPropertyName("month")]
    public string Month { get; set; } = string.Empty;

    [JsonPropertyName("memberCount")]
    public int MemberCount { get; set; }

    [JsonPropertyName("newMembers")]
    public int NewMembers { get; set; }
}