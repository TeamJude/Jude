using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Domains.Auth.Authorization;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid roleId, string feature, Permission requiredPermission);
}

public class PermissionService(JudeDbContext repository) : IPermissionService
{
    private readonly JudeDbContext _repository = repository;

    public async Task<bool> HasPermissionAsync(
        Guid roleId,
        string feature,
        Permission requiredPermission
    )
    {
        var userPermissions = await _repository
            .Roles.AsNoTracking()
            .Where(r => r.Id == roleId)
            .Select(r => r.Permissions)
            .FirstOrDefaultAsync();

        if (userPermissions == null)
        {
            return false;
        }

        foreach (var permissionSet in userPermissions)
        {
            if (permissionSet.TryGetValue(feature, out var userPermission))
            {
                if (userPermission >= requiredPermission)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
