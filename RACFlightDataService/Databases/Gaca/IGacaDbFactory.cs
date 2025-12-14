using System.Data;

namespace RACFlightDataService.Databases.Gaca;

public interface IGacaDbFactory
{
    IDbConnection CreateConnection();
}