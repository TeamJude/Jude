using Jude.Server.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Data.Repository;

public class JudeDbContext(DbContextOptions<JudeDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<UserModel> Users { get; set; }
}
