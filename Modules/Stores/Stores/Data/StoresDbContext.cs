using System.Reflection;

namespace Stores.Data;

public class StoresDbContext : DbContext
{
    public StoresDbContext(DbContextOptions<StoresDbContext> options) : base(options)
    {
    }
    
    public DbSet<Store> Stores { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("stores");
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}