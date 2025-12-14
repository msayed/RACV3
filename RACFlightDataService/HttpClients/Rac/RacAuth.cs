using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using RACFlightDataService.HttpClients.Base;
using RACFlightDataService.Logging;
using RACFlightDataService.Options;
using RestSharp;

namespace RACFlightDataService.HttpClients.Rac;

public class RacAuth : IRacAuth
{
    private readonly ILoggerAdapter<RacAuth> _logger;
    private readonly RACOptions _options;
    private RacToken _racToken;

    public RacAuth(IOptions<RACOptions> options,ILoggerAdapter<RacAuth> logger)
    {
        _logger = logger;
        _options = options.Value;
      
    }

    public async  Task<Result<RacToken>> GetAuthTokenAsync(IBaseRestClient httpClient, CancellationToken stoppingToken)
    {
        try
        {
            
            _logger.LogInformation(JsonSerializer.Serialize(_racToken));
            if (_racToken != null && DateTime.Now.AddSeconds(-100) <= _racToken.ExpireIn) 
            {
                _logger.LogInformation("token is not expire yet");
                return _racToken;
            }

            var request = new RestRequest(_options.AuthApiEndPoint, Method.POST);
            request.AddParameter("application/x-www-form-urlencoded", BuildUrlForOauth2(), ParameterType.RequestBody);

            //ByPass SSL Validation for AUTH2
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var response = await httpClient.ExecuteWithLog(request, stoppingToken);
            if (!response.IsSuccessful)
            {
                var exp=new InvalidOperationException("Failed to get auth token",response.ErrorException);
                return new Result<RacToken>(exp);
            }
            var deserializedResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response.Content);
            if (deserializedResponse == null)
            {
                var exp=new InvalidOperationException("Failed to get auth token");
                return new Result<RacToken>(exp);
            }

            _racToken = new RacToken(
                deserializedResponse["access_token"].ToString(),
                DateTime.Now.AddSeconds(Convert.ToInt32(deserializedResponse["expires_in"].ToString()))
            );

            return _racToken;
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Failed to get auth token for Rac service");
            return new Result<RacToken>(e);
        }

    }

    private string BuildUrlForOauth2()
    {
        StringBuilder strUrl = new StringBuilder();
        strUrl.AppendFormat("{0}={1}", "grant_type", "client_credentials");
        strUrl.AppendFormat("&{0}={1}", "client_id", _options.AuthClientId);
        strUrl.AppendFormat("&{0}={1}", "client_secret", _options.AuthClientSecret);
        return strUrl.ToString();
    }
}