using Microsoft.AspNetCore.Http;
using Jude.Server.Data.Models;

namespace Jude.Server.Domains.Policies;

public record PolicyUploadRequest(string Name, IFormFile File);

public record GetPoliciesRequest(int Page = 1, int PageSize = 10);

public record GetPoliciesResponse(PolicyResponse[] Policies, int TotalCount);

public record PolicyResponse(
    int Id,
    string Name,
    string DocumentId,
    string DocumentUrl,
    PolicyStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedById
); 