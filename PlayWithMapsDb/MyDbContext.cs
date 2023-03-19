namespace PlayWithMaps.Db;

public class MyDbContext : DbContext
{
    private readonly IPath _path;

    public MyDbContext(IPath path, DbContextOptions<MyDbContext> options)
        : base(options)
    {
        _path = path;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite($"Filename={_path.GetDatabasePath()}");
    }

    public DbSet<PositionRec> PositionRec { get; set; } = null!;
    public DbSet<Position> Position { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<PositionRec>().HasMany(x => x.Positions).WithOne(x => x.PositionRec);
    }
}