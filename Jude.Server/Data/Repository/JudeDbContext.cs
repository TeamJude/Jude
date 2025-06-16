using Jude.Server.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Data.Repository;

public class JudeDbContext(DbContextOptions<JudeDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //configure the permissions prop to be a json object
        modelBuilder
            .Entity<RoleModel>()
            .Property(r => r.Permissions)
            .HasColumnType("jsonb")
            .IsRequired();
    }

    public DbSet<UserModel> Users { get; set; }

    public DbSet<RoleModel> Roles { get; set; }
    public DbSet<RuleModel> Rules { get; set; }

}

