using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RACFlightDataService.Logging;
using RestSharp;

namespace RACFlightDataService.HttpClients.Base
{
    public abstract class BaseRestClient<T> : RestClient, IBaseRestClient
    {
        protected readonly ILoggerAdapter<T> _logger;
        protected int _timeout=-1 ;

        protected BaseRestClient(string baseUrl, ILoggerAdapter<T> logger)
        {
            _logger = logger;
            this.BaseUrl = new Uri(baseUrl);
            _logger.SetServiceName(typeof(T).Name);
        }
        public async Task<IRestResponse> ExecuteWithLog(IRestRequest request, CancellationToken stoppingToken)
        {
            Log(request);
            this.Timeout = _timeout;
            var response = await this.ExecuteAsync(request, stoppingToken);
            Log(response);
            return response;
        }


        private void Log(IRestRequest request)
        {
            var authToken = GetParameter(request, "Authorization");
            var transactionId = GetParameter(request, "transaction-id");
            if (string.IsNullOrEmpty(transactionId))
            {
                _logger.LogInformation("Calling {Url} with content => {Content}", request.Resource,
                    ReadContent(request.Parameters));
            }
            else
            {
                _logger.LogInformation("Calling {Url} TransId: {Uuid} ({AuthToken}) with content => {Content}",
                    request.Resource,
                    transactionId, authToken, ReadContent(request.Parameters));
            }
        }

        private void Log(IRestResponse response)
        {
            var request = response.Request;
            var responseString = response.Content;

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                _logger.LogInformation("Response for {Url} is ({Status}) with content => {Content}",
                    request.Resource, response.StatusCode, responseString);
                return;
            }

            _logger.LogError("Response failed => {Message}", response.ErrorMessage);

            var authToken = GetParameter(request, "Authorization");
            var transactionId = GetParameter(request, "transaction-id");

            if (string.IsNullOrEmpty(transactionId))
            {
                _logger.LogError("Response for {Url} is ({Status}) with content => {Content}",
                    request.Resource, response.StatusCode.ToString(), responseString);
            }
            else
            {
                _logger.LogError(
                    "Response for {Url} is ({Status}) TransId: {Uuid} ({AuthToken}) with content => {Content}",
                    request.Resource, response.StatusCode.ToString(), transactionId, authToken, responseString);
            }
        }

        private string GetParameter(IRestRequest request, string paramName)
        {
            var value = request.Parameters.Find(a => a.Name == paramName)?.Value;
            return (value ?? "").ToString();
        }

        private string ReadContent(List<Parameter> parameters)
        {
            var json = parameters.Find(z => z.Type == ParameterType.RequestBody);
            if (json == null)
            {
                var query = parameters.Find(z => z.Name == "application/x-www-form-urlencoded");
                return query != null ? (string)query.Value : "";
            }
            else
            {
                return json.Value.ToString();
            }
        }
    }
}