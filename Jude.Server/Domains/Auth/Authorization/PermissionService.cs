using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Jude.Server.Domains.Auth.Authorization;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid roleId, string feature, Permission requiredPermission);
    void InvalidateRoleCacheAsync(Guid roleId);
}

public class PermissionService : IPermissionService
{
    private readonly JudeDbContext _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PermissionService> _logger;

    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(30);
    private const string ROLE_PERMISSIONS_KEY_PREFIX = "role_permissions_";

    public PermissionService(
        JudeDbContext repository,
        IMemoryCache cache,
        ILogger<PermissionService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(
        Guid roleId,
        string feature,
        Permission requiredPermission
    )
    {
        var rolePermissions = await GetRolePermissionsAsync(roleId);

        if (rolePermissions == null)
        {
            return false;
        }

        if (rolePermissions.TryGetValue(feature, out var userPermission))
        {
            return userPermission >= requiredPermission;
        }

        return false;
    }

    public void InvalidateRoleCacheAsync(Guid roleId)
    {
        var cacheKey = $"{ROLE_PERMISSIONS_KEY_PREFIX}{roleId}";
        _cache.Remove(cacheKey);
        _logger.LogDebug("Invalidated permission cache for role {RoleId}", roleId);
    }

    private async Task<Dictionary<string, Permission>?> GetRolePermissionsAsync(Guid roleId)
    {
        var cacheKey = $"{ROLE_PERMISSIONS_KEY_PREFIX}{roleId}";

        if (_cache.TryGetValue(cacheKey, out Dictionary<string, Permission>? cachedPermissions))
        {
            _logger.LogDebug("Retrieved permissions from cache for role {RoleId}", roleId);
            return cachedPermissions;
        }

        _logger.LogDebug("Loading permissions from database for role {RoleId}", roleId);

        var rolePermissions = await _repository
            .Roles
            .AsNoTracking()
            .Where(r => r.Id == roleId)
            .Select(r => r.Permissions)
            .FirstOrDefaultAsync();

        if (rolePermissions != null)
        {
            _cache.Set(cacheKey, rolePermissions, CacheExpiry);
            _logger.LogDebug("Cached permissions for role {RoleId}", roleId);
        }

        return rolePermissions;
    }
}
