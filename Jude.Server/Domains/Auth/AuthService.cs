using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;

namespace Jude.Server.Domains.Auth;

public interface IAuthService
{
    public Task<Result<AuthResponse>> Register(RegisterRequest request);
    public Task<Result<AuthResponse>> Login(LoginRequest request);
    public Task<Result<UserDataResponse>> GetUserData(Guid userId);
}

public class AuthService : IAuthService
{
    private readonly JudeDbContext _repository;

    //using inbuilt password hasher
    private readonly IPasswordHasher<UserModel> _passwordHasher;
    private readonly ITokenProvider _jwt;

    public AuthService(
        JudeDbContext repository,
        IPasswordHasher<UserModel> passwordHasher,
        ITokenProvider jwt
    )
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    public async Task<Result<AuthResponse>> Login(LoginRequest request)
    {
        // get the user associated with the identifier
        var user = await _repository
            .Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u =>
                u.Email == request.UserIdentifier || u.Username == request.UserIdentifier
            );

        if (user == null)
            return Result.Fail("Invalid credentials");

        if (user.AuthProvider != AuthProvider.Email)
            return Result.Fail($"Please sign in with {user.AuthProvider.GetDisplayName()}");

        // Verify password
        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
            return Result.Fail("Invalid credentials");

        var token = _jwt.GenerateToken(user.Id.ToString(), user.RoleId.ToString());

        var role = new UserRole(user.Role.Name, user.Role.Permissions);

        var userData = new UserDataResponse(
            user.Id,
            user.Email,
            user.Username,
            user.AvatarUrl,
            user.CreatedAt,
            role
        );

        return Result.Ok(new AuthResponse(token, userData));
    }

    public async Task<Result<AuthResponse>> Register(RegisterRequest request)
    {
        var isEmailTaken = await _repository.Users.AnyAsync(u => u.Email == request.Email);
        if (isEmailTaken)
            return Result.Fail("This email is already registered");

        var validationResult = new RegistrationValidator().Validate(request);
        if (!validationResult.IsValid)
            return Result.Fail(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        RoleModel role;

        // Check if role exists
        var existingRole = await _repository.Roles
            .FirstOrDefaultAsync(r => r.Name == request.RoleName);

        if (existingRole != null)
        {
            // If role exists but permissions are provided, return error
            if (request.Permissions != null)
                return Result.Fail("Cannot specify permissions when using an existing role");

            role = existingRole;
        }
        else
        {
            // If role doesn't exist, permissions must be provided
            if (request.Permissions == null)
                return Result.Fail("Permissions must be provided when creating a new role");

            role = new RoleModel
            {
                Name = request.RoleName,
                Permissions = request.Permissions
            };
        }

        var user = new UserModel
        {
            Username = request.Username,
            Email = request.Email,
            AuthProvider = AuthProvider.Email,
            Role = role
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _repository.Users.AddAsync(user);
        await _repository.SaveChangesAsync();

        var token = _jwt.GenerateToken(user.Id.ToString(), user.RoleId.ToString());
        var userData = new UserDataResponse(
            user.Id,
            user.Email,
            user.Username,
            user.AvatarUrl,
            user.CreatedAt,
            new(user.Role.Name, user.Role.Permissions)
        );

        return Result.Ok(new AuthResponse(token, userData));
    }

    public async Task<Result<UserDataResponse>> GetUserData(Guid userId)
    {
        var user = await _repository
            .Users.Where(u => u.Id == userId)
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync();

        if (user == null)
            return Result.Fail("User not found");

        var userData = new UserDataResponse(
            user.Id,
            user.Email,
            user.Username,
            user.AvatarUrl,
            user.CreatedAt,
            new(user.Role.Name, user.Role.Permissions)
        );

        return Result.Ok(userData);
    }
}
