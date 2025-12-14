using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RACFlightDataService.Databases.Gaca.Models;

namespace RACFlightDataService.Databases.Gaca;

public interface IGacaRepository
{
    Task<IEnumerable<FlightDto>> GetFlightsDataAsync(DateTime date, CancellationToken cancellationToken);
}