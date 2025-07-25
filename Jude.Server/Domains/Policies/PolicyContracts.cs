using Microsoft.AspNetCore.Http;

namespace Jude.Server.Domains.Policies;

public record PolicyUploadRequest(string Name, IFormFile File); 