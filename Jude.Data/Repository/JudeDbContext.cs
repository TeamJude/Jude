using Microsoft.EntityFrameworkCore;
using Jude.Data.Models;

namespace Jude.Data.Repository;

public class JudeDbContext(DbContextOptions<JudeDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> Users { get; set; }
}
