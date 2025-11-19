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

        // Configure ClaimModel indexes
        modelBuilder.Entity<ClaimModel>().HasIndex(c => c.ClaimLineNo).IsUnique();
        modelBuilder.Entity<ClaimModel>().HasIndex(c => c.Status);
        modelBuilder.Entity<ClaimModel>().HasIndex(c => c.IngestedAt);
        
        // Search optimization indexes
        modelBuilder.Entity<ClaimModel>().HasIndex(c => c.ClaimNumber);
        modelBuilder.Entity<ClaimModel>().HasIndex(c => c.MemberNumber);
        modelBuilder.Entity<ClaimModel>().HasIndex(c => c.PatientFirstName);
        modelBuilder.Entity<ClaimModel>().HasIndex(c => c.PatientSurname);
        modelBuilder.Entity<ClaimModel>().HasIndex(c => c.ProviderName);
        modelBuilder.Entity<ClaimModel>().HasIndex(c => c.PracticeNumber);

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

        // Configure AuditLogModel indexes and JSONB
        modelBuilder.Entity<AuditLogModel>().HasIndex(a => new { a.EntityType, a.EntityId });
        modelBuilder.Entity<AuditLogModel>().HasIndex(a => a.Timestamp);
        modelBuilder
            .Entity<AuditLogModel>()
            .Property(a => a.Metadata)
            .HasColumnType("jsonb");
    }

    public DbSet<UserModel> Users { get; set; }
    public DbSet<RoleModel> Roles { get; set; }
    public DbSet<RuleModel> Rules { get; set; }
    public DbSet<FraudIndicatorModel> FraudIndicators { get; set; }
    public DbSet<ClaimModel> Claims { get; set; }
    public DbSet<AgentReviewModel> AgentReviews { get; set; }
    public DbSet<HumanReviewModel> HumanReviews { get; set; }
    public DbSet<PolicyModel> Policies { get; set; }
    public DbSet<AuditLogModel> AuditLogs { get; set; }
}
