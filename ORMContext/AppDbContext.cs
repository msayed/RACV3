using Microsoft.EntityFrameworkCore;
using ORMContext.Domain;

namespace ORMContext;

public class AppDbContext:DbContext,IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Flight> Flights => base.Set<Flight>();
    public DbSet<FlightHistory> FlightHistories => base.Set<FlightHistory>();
    
    
}

// public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
// {
//     private readonly IConfiguration _configuration;
//
//     public AppDbContextFactory(IConfiguration configuration)
//     {
//         _configuration = configuration;
//     }
//     public AppDbContext CreateDbContext(string[] args)
//     {
//         var connection = _configuration.GetConnectionString("RacDbConnection");
//         var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
//         optionsBuilder.UseSqlServer(connection);
//
//         return new AppDbContext(optionsBuilder.Options);
//     }
// }