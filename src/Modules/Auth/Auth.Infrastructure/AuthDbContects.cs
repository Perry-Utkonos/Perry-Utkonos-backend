using Microsoft.EntityFrameworkCore;
using Auth.Domain;

namespace Auth.Infrastructure;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth"); // Своя схема в PostgreSQL
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
    }
}
