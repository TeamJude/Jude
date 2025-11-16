using Jude.Server.Data.Models;

namespace Jude.Server.Domains.Agents.Events;

public record ClaimIngestEvent(ClaimModel Claim, DateTime IngestedAt);
