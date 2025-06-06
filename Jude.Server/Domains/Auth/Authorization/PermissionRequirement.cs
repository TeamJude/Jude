using Jude.Server.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace Jude.Server.Domains.Auth.Authorization;

public class PermissionRequirement(string feature, Permission permission)
    : IAuthorizationRequirement
{
    public (string feature, Permission permission) RequiredPermission { get; set; } =
        (feature, permission);
}
