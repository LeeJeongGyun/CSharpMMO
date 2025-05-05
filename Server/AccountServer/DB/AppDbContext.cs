using Microsoft.EntityFrameworkCore;

namespace AccountServer.DB;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AccountDb> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountDb>()
            .HasIndex(a => a.AccountName)
            .IsUnique();
    }
}
