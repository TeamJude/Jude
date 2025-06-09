using System;
using System.Threading.Tasks;
using Jude.Server.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Data.Repository;

public static class DbSeeder
{
    public static async Task SeedData(
        JudeDbContext context,
        IPasswordHasher<UserModel> passwordHasher
    )
    {
        var anyUsers = await context.Users.AnyAsync();
        if (anyUsers)
            return;
        var adminRole = new RoleModel
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Permissions =
            [
                new()
                {
                    { "Claims", Permission.Write },
                    { "Users", Permission.Write },
                    { "Roles", Permission.Write },
                },
            ],
        };

        var userRole = new RoleModel
        {
            Id = Guid.NewGuid(),
            Name = "User",
            Permissions = [new() { { "Claims", Permission.Read } }],
        };

        await context.Roles.AddRangeAsync(adminRole, userRole);
        await context.SaveChangesAsync();

        var adminUser = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@jude.com",
            EmailConfirmedAt = DateTime.UtcNow,
            AuthProvider = AuthProvider.Email,
            RoleId = adminRole.Id,
            Role = adminRole,
            CreatedAt = DateTime.UtcNow,
        };

        var regularUser = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "user",
            Email = "user@jude.com",
            EmailConfirmedAt = DateTime.UtcNow,
            AuthProvider = AuthProvider.Email,
            RoleId = userRole.Id,
            Role = userRole,
            CreatedAt = DateTime.UtcNow,
        };

        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin123!");
        regularUser.PasswordHash = passwordHasher.HashPassword(regularUser, "User123!");

        await context.Users.AddRangeAsync(adminUser, regularUser);
        await context.SaveChangesAsync();
    }
}
