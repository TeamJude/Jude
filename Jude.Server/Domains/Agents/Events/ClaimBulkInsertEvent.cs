using Jude.Server.Data.Models;

namespace Jude.Server.Domains.Agents.Events;

public record ClaimBulkInsertEvent(List<ClaimModel> Claims, string FileName, DateTime IngestedAt);

