using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Common;
using RACFlightDataService.HttpClients.Base;
using RestSharp;

namespace RACFlightDataService.HttpClients.Rac;

public interface IRacAuth
{
    Task<Result<RacToken>> GetAuthTokenAsync(IBaseRestClient httpClient, CancellationToken stoppingToken);
}