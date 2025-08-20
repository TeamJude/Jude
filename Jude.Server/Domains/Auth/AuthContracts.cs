using Jude.Server.Data.Models;

namespace Jude.Server.Domains.Auth;

//for DTOs we use for auth, could have named this AuthDTOs but AuthContracts is more aesthetic imo
public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string RoleName,
    Dictionary<string, Permission>? Permissions = null
);

public record LoginRequest(string UserIdentifier, string Password);

public record AuthResponse(string Token, UserDataResponse UserData);

public record UserDataResponse(
    Guid Id,
    string Email,
    string? Username,
    string? AvatarUrl,
    DateTime CreatedAt,
    UserRole Role,
    string token
);

public record UserRole(string Name, Dictionary<string, Permission> Permissions);
