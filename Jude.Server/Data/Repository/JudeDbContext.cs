using Jude.Server.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Data.Repository;

public class JudeDbContext(DbContextOptions<JudeDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the permissions property to be a json object with the new Dictionary structure
        modelBuilder
            .Entity<RoleModel>()
            .Property(r => r.Permissions)
            .HasColumnType("jsonb")
            .IsRequired();

        modelBuilder.Entity<RoleModel>().HasIndex(r => r.Name).IsUnique();

        modelBuilder.Entity<UserModel>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<UserModel>().HasIndex(u => u.Username).IsUnique();

        // Configure ClaimModel Data property as JSONB
        modelBuilder
            .Entity<ClaimModel>()
            .Property(c => c.Data)
            .HasColumnType("jsonb")
            .IsRequired();

        // Configure one-to-one relationships for ClaimModel
        modelBuilder.Entity<ClaimModel>()
            .HasOne(c => c.AgentReview)
            .WithOne(ar => ar.Claim)
            .HasForeignKey<AgentReviewModel>(ar => ar.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClaimModel>()
            .HasOne(c => c.HumanReview)
            .WithOne(hr => hr.Claim)
            .HasForeignKey<HumanReviewModel>(hr => hr.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClaimModel>()
            .HasOne(c => c.Summary)
            .WithOne(cs => cs.Claim)
            .HasForeignKey<ClaimSummaryModel>(cs => cs.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<UserModel> Users { get; set; }
    public DbSet<RoleModel> Roles { get; set; }
    public DbSet<RuleModel> Rules { get; set; }
    public DbSet<FraudIndicatorModel> FraudIndicators { get; set; }
    public DbSet<ClaimModel> Claims { get; set; }
    public DbSet<AgentReviewModel> AgentReviews { get; set; }
    public DbSet<HumanReviewModel> HumanReviews { get; set; }
    public DbSet<ClaimSummaryModel> ClaimSummaries { get; set; }
    public DbSet<PolicyModel> Policies { get; set; }

}
