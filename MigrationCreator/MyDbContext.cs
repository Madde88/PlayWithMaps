using Microsoft.EntityFrameworkCore;
using PlayWithMaps.Models;

namespace PlayWithMaps.Db;

public class MyDbContext : DbContext
{

    private const string DatabaseName = "MyDb.db";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Filename={DatabaseName}");
    }

    public DbSet<PositionRec> PositionRec { get; set; } = null!;
    public DbSet<Position> Position { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<PositionRec>().HasMany(x => x.Positions).WithOne(x => x.PositionRec);
    }
}