namespace Jude.Server.Domains.Auth;

//for DTOs we use for auth, could have named this AuthDTOs but AuthContracts is more aesthetic imo
public record RegisterRequest(string Username, string Email, string Password);

public record LoginRequest(string UserIdentifier, string Password);

public record AuthResponse(string Token, UserDataResponse UserData);

public record UserDataResponse(
    Guid Id,
    string Email,
    string? Username,
    string? AvatarUrl,
    DateTime CreatedAt
);
