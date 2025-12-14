using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using Serilog;

namespace RACFlightDataService {
  public static class HttpClientExt {
    public static ILogger Logger;

    public static async Task<IRestResponse> LogAndExecute(this IRestClient httpClient, IRestRequest request, CancellationToken stoppingToken) {
      // Log(request);
      httpClient.Timeout = -1;
      var response = await httpClient.ExecutePostAsync(request, stoppingToken);
      // Log(response);
      return response;
    }


    private static void Log(IRestRequest request) {
      var authToken = GetParameter(request, "Authorization");
      var transactionId = GetParameter(request, "transaction-id");

      if (string.IsNullOrEmpty(transactionId)) {
        Logger.Information("Calling {Url} with content => {Content}", request.Resource, ReadContent(request.Parameters));
      }
      else {
        Logger.Information("Calling {Url} TransId: {Uuid} ({AuthToken}) with content => {Content}", request.Resource,
          transactionId, authToken, ReadContent(request.Parameters));
      }
    }

    private static void Log(IRestResponse response) {
      var request = response.Request;
      var responseString = response.Content;

      if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created) {
        Logger.Information("Response for {Url} is ({Status}) with content => {Content}",
          request.Resource, response.StatusCode, responseString);
        return;
      }

      Logger.Error("Response failed => {Message}", response.ErrorMessage);

      var authToken = GetParameter(request, "Authorization");
      var transactionId = GetParameter(request, "transaction-id");

      if (string.IsNullOrEmpty(transactionId)) {
        Logger.Error("Response for {Url} is ({Status}) with content => {Content}",
          request.Resource, response.StatusCode.ToString(), responseString);
      }
      else {
        Logger.Error("Response for {Url} is ({Status}) TransId: {Uuid} ({AuthToken}) with content => {Content}",
          request.Resource, response.StatusCode.ToString(), transactionId, authToken, responseString);
      }
    }

    private static string GetParameter(IRestRequest request, string paramName) {
      var value = request.Parameters.Find(a => a.Name == paramName)?.Value;
      return (value ?? "").ToString();
    }

    private static string ReadContent(List<Parameter> parameters) {
      var json = parameters.Find(z => z.Type == ParameterType.RequestBody);
      if (json == null) {
        var query = parameters.Find(z => z.Name == "application/x-www-form-urlencoded");
        return query != null ? (string)query.Value : "";
      }
      else {
        return json.Value.ToString();
      }
    }
  }
}