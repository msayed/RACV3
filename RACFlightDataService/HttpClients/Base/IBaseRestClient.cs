using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace RACFlightDataService.HttpClients.Base;

public interface IBaseRestClient
{
    Task<IRestResponse> ExecuteWithLog(IRestRequest request, CancellationToken stoppingToken);
}