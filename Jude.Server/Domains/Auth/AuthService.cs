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
}

public class AuthService : IAuthService
{
    private readonly JudeDbContext _repository;

    //using inbuilt password hasher
    private readonly IPasswordHasher<UserModel> _passwordHasher;
    private readonly IJwtTokenManager _jwt;

    public AuthService(
        JudeDbContext repository,
        IPasswordHasher<UserModel> passwordHasher,
        IJwtTokenManager jwt
    )
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    public async Task<Result<AuthResponse>> Login(LoginRequest request)
    {
        // get the user associated with the identifier
        var user = await _repository.Users.FirstOrDefaultAsync(u =>
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

        var token = _jwt.GenerateToken(user.Id.ToString(), user.Email);
        var userData = new UserDataResponse(
            user.Id,
            user.Email,
            user.Username,
            user.AvatarUrl,
            user.CreatedAt
        );

        return Result.Ok(new AuthResponse(token, userData));
    }

    public async Task<Result<AuthResponse>> Register(RegisterRequest request)
    {
        var isEmailTaken = await _repository.Users.AnyAsync(u => u.Email == request.Email);
        if (isEmailTaken)
            return Result.Fail("Email already exists");

        var validationResult = new RegistrationValidator().Validate(request);

        if (!validationResult.IsValid)
            return Result.Fail(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var user = new UserModel
        {
            Username = request.Username,
            Email = request.Email,
            AuthProvider = AuthProvider.Email,
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _repository.Users.AddAsync(user);
        await _repository.SaveChangesAsync();

        var token = _jwt.GenerateToken(user.Id.ToString(), user.Email);
        var userData = new UserDataResponse(
            user.Id,
            user.Email,
            user.Username,
            user.AvatarUrl,
            user.CreatedAt
        );

        return Result.Ok(new AuthResponse(token, userData));
    }
}
