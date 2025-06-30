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
            Permissions = new()
            {
                { "Claims", Permission.Write },
                { "Users", Permission.Write },
                { "Roles", Permission.Write },
                { "Rules", Permission.Write },
                { "Fraud", Permission.Write },
                { "Dashboard", Permission.Write },
            },
        };

        var userRole = new RoleModel
        {
            Id = Guid.NewGuid(),
            Name = "User",
            Permissions = new()
            {
                { "Claims", Permission.Read },
                { "Dashboard", Permission.Read },
            },
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

        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "hro");
        regularUser.PasswordHash = passwordHasher.HashPassword(regularUser, "User123!");

        await context.Users.AddRangeAsync(adminUser, regularUser);
        await context.SaveChangesAsync();

        // Seed sample claims for demo
        await SeedSampleClaims(context);
    }

    private static async Task SeedSampleClaims(JudeDbContext context)
    {
        var anyClaims = await context.Claims.AnyAsync();
        if (anyClaims)
            return;

        var sampleClaims = new[]
        {
            new ClaimModel
            {
                Id = Guid.NewGuid(),
                TransactionNumber = $"TN-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
                ClaimNumber = "CLM-001",
                PatientName = "JOHNSON, Emily",
                MembershipNumber = "11098765-2",
                ProviderPractice = "54321",
                ClaimAmount = 2450.00m,
                Currency = "USD",
                Status = ClaimStatus.Pending,
                Source = ClaimSource.CIMAS,
                IngestedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-2),
                FraudRiskLevel = FraudRiskLevel.Medium,
                IsFlagged = false,
                RequiresHumanReview = true
            },
            new ClaimModel
            {
                Id = Guid.NewGuid(),
                TransactionNumber = $"TN-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
                ClaimNumber = "CLM-002",
                PatientName = "SMITH, John",
                MembershipNumber = "11153422-1",
                ProviderPractice = "67890",
                ClaimAmount = 875.50m,
                Currency = "USD",
                Status = ClaimStatus.Pending,
                Source = ClaimSource.CIMAS,
                IngestedAt = DateTime.UtcNow.AddHours(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-1),
                FraudRiskLevel = FraudRiskLevel.Low,
                IsFlagged = false,
                RequiresHumanReview = true
            },
            new ClaimModel
            {
                Id = Guid.NewGuid(),
                TransactionNumber = $"TN-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
                ClaimNumber = "CLM-003",
                PatientName = "WILLIAMS, Robert",
                MembershipNumber = "11234567-0",
                ProviderPractice = "98765",
                ClaimAmount = 125.75m,
                Currency = "USD",
                Status = ClaimStatus.Pending,
                Source = ClaimSource.CIMAS,
                IngestedAt = DateTime.UtcNow.AddMinutes(-30),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-30),
                FraudRiskLevel = FraudRiskLevel.Low,
                IsFlagged = false,
                RequiresHumanReview = true
            },
            new ClaimModel
            {
                Id = Guid.NewGuid(),
                TransactionNumber = $"TN-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
                ClaimNumber = "CLM-004",
                PatientName = "BROWN, Sarah",
                MembershipNumber = "11345678-1",
                ProviderPractice = "11111",
                ClaimAmount = 3200.00m,
                Currency = "USD",
                Status = ClaimStatus.PendingReview,
                Source = ClaimSource.CIMAS,
                IngestedAt = DateTime.UtcNow.AddHours(-4),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-15),
                ProcessedAt = DateTime.UtcNow.AddMinutes(-20),
                FraudRiskLevel = FraudRiskLevel.High,
                IsFlagged = true,
                RequiresHumanReview = true,
                AgentRecommendation = "Review Required",
                AgentReasoning = "High claim amount detected. Unusual billing pattern identified. Provider history requires review.",
                AgentConfidenceScore = 0.85m,
                AgentProcessedAt = DateTime.UtcNow.AddMinutes(-20)
            },
            new ClaimModel
            {
                Id = Guid.NewGuid(),
                TransactionNumber = $"TN-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
                ClaimNumber = "CLM-005",
                PatientName = "DAVIS, Michael",
                MembershipNumber = "11456789-3",
                ProviderPractice = "22222",
                ClaimAmount = 450.25m,
                Currency = "USD",
                Status = ClaimStatus.PendingReview,
                Source = ClaimSource.CIMAS,
                IngestedAt = DateTime.UtcNow.AddHours(-6),
                UpdatedAt = DateTime.UtcNow.AddHours(-1),
                ProcessedAt = DateTime.UtcNow.AddHours(-1),
                FraudRiskLevel = FraudRiskLevel.Low,
                IsFlagged = false,
                RequiresHumanReview = true,
                AgentRecommendation = "Approve",
                AgentReasoning = "All validation checks passed. Standard claim with appropriate medical codes. Provider in good standing.",
                AgentConfidenceScore = 0.95m,
                AgentProcessedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        await context.Claims.AddRangeAsync(sampleClaims);
        await context.SaveChangesAsync();
    }
}
