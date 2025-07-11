using Jude.Server.Domains.Claims.Providers.CIMAS;

namespace Jude.Server.Domains.Agents.Events;

public record ClaimIngestEvent(
    string TransactionNumber,
    ClaimResponse CIMASClaimData,
    DateTime IngestedAt,
    string Source = "CIMAS"
);
