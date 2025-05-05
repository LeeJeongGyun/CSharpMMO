namespace Server.DB;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Data;

public class AppDbContext : DbContext
{
    private const string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=GameDb;";

    private static readonly ILoggerFactory _logger = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

    public DbSet<AccountDb> Accounts { get; set; }

    public DbSet<PlayerDb> Players { get; set; }

    public DbSet<ItemDb> Items { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options
            //.UseLoggerFactory(_logger)
            .UseSqlServer(ConfigManager.Config == null ? _connectionString : ConfigManager.Config.dbConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<AccountDb>()
            .HasIndex(account => account.AccountName)
            .IsUnique();

        builder.Entity<PlayerDb>()
            .HasIndex(player => player.PlayerName)
            .IsUnique();

        // Entity 클래스가 아닌 클래스를 Db Table에 매핑
        builder.Entity<PlayerDb>()
            .OwnsOne(p => p.StatInfo);
    }
}
