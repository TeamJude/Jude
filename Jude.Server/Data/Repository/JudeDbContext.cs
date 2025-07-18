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
    }

    public DbSet<UserModel> Users { get; set; }
    public DbSet<RoleModel> Roles { get; set; }
    public DbSet<RuleModel> Rules { get; set; }
    public DbSet<FraudIndicatorModel> FraudIndicators { get; set; }
    public DbSet<ClaimModel> Claims { get; set; }
}
