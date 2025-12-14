using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using RACFlightDataService.HttpClients.Base;
using RACFlightDataService.HttpClients.Rac.Requests;
using RACFlightDataService.Logging;
using RACFlightDataService.Options;
using RestSharp;

namespace RACFlightDataService.HttpClients.Rac;

public class TavRestClient : BaseRestClient<TavRestClient>
{
    private readonly ILoggerAdapter<TavRestClient> _logger;

    public TavRestClient(IOptions<RACOptions> options, ILoggerAdapter<TavRestClient> logger) :
        base(options.Value.TavAutoProcessUrl, logger)
    {
        _logger = logger;
    }

    public async Task<Result<IRestResponse>> UpdateTavAutoService(
        CancellationToken cancellationToken = default)
    {
        try
        {
         //   this.BaseUrl= new Uri("https://portal.nasaviation.com/TavAutoProcess/Default.aspx");
            // using var webClient = new WebClient();
            // webClient.BaseAddress = this.BaseUrl.ToString();
            // webClient.he
            // var result=webClient.DownloadString(this.BaseUrl.ToString());
            var requestTimout = 600000;
            this.Timeout = requestTimout;
            var request = new RestRequest( Method.GET);
            request.Timeout = requestTimout;
            request.AddHeader("Accept", "*/*");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36 Edg/105.0.1343.33");
                // ,TimeSpan.FromMinutes(10).Milliseconds
            this._timeout = requestTimout;
            this.ReadWriteTimeout = requestTimout;
            var response = await this.ExecuteWithLog(request,cancellationToken);
            return (RestResponse)response;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update tavprocess");
            return new Result<IRestResponse>(e);
        }
    }
}