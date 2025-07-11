namespace Jude.Server.Domains.User;

public record UserResponse(Guid Id, string AvatarUrl, string Username, string Email, string Role);
