using Microsoft.EntityFrameworkCore;
using Steamstatus.Infrastructure.Telegram;

namespace Steamstatus.db;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<TelegramModel> SubNotifies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelegramModel>().HasIndex(x =>
            new { x.ChatId, x.ServiceName }).IsUnique();
    }
}