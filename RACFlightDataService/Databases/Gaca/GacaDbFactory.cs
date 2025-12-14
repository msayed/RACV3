using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace RACFlightDataService.Databases.Gaca;

public class GacaDbFactory:IGacaDbFactory
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;
    public GacaDbFactory(IConfiguration configuration) {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DbConnection");
    }
    public IDbConnection CreateConnection()
        => new SqlConnection(_connectionString);
}