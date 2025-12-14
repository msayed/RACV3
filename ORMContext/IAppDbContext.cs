using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ORMContext.Domain;

namespace ORMContext;

public interface IAppDbContext
{
    
     DbSet<Flight> Flights { get;  }
     DbSet<FlightHistory> FlightHistories { get; }
     
         
     Task<int> SaveChangesAsync(CancellationToken cancellationToken=default);
     DatabaseFacade Database { get; }
}