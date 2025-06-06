using Jude.Server.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Extensions;

namespace Jude.Server.Domains.Auth.Authorization;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string feature, Permission permission)
        : base($"{feature}.{permission.GetDisplayName()}") { }
}
