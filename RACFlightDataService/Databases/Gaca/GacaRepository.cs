using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using RACFlightDataService.Databases.Gaca.Models;
using RACFlightDataService.Logging;

namespace RACFlightDataService.Databases.Gaca;

public class GacaRepository : IGacaRepository
{
    private readonly ILoggerAdapter<GacaRepository> _logger;
    private readonly IGacaDbFactory _context;
    private readonly string _sqlFilePath;

    public GacaRepository(ILoggerAdapter<GacaRepository> logger, IGacaDbFactory context, IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        var path = configuration.GetSection("SqlPath").Get<string>();
        _sqlFilePath = !string.IsNullOrEmpty(path) ? path : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sql");
    }

    public async Task<IEnumerable<FlightDto>> GetFlightsDataAsync(DateTime date, CancellationToken cancellationToken)
    {
        try
        {
            var sectordate = date.ToString("yyyy-MM-dd");
            var queryFilePath = Path.Combine(_sqlFilePath, "RAC.sql");
            var query = await File.ReadAllTextAsync(queryFilePath,cancellationToken);
            var command = new CommandDefinition(query, new { sectordate }, cancellationToken: cancellationToken);
            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<FlightDto>(command);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetFlightsDataAsync ->Failed to get data from GACA database ");
            return Enumerable.Empty<FlightDto>();
        }
    }
}