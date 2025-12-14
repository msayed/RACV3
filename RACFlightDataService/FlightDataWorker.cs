using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RACAPI.Models;
using RACFlightDataService.HttpClients.Rac;
using RACFlightDataService.Options;
using RestSharp;

namespace RACFlightDataService {
  public record RacToken {
    public RacToken(string authToken, DateTime expireIn) {
      AuthToken = authToken;
      ExpireIn = expireIn;
    }

    public string AuthToken { get; }
    public DateTime ExpireIn { get; }
  }

  public class FlightDataWorker : BackgroundService {
    private readonly ILogger<FlightDataWorker> _logger;
    private readonly Database _db;
    private readonly RACOptions _racOptions;
    private readonly RacRestClient _httpClient;
    private RacToken RacToken { get; set; }

    public FlightDataWorker(
      ILogger<FlightDataWorker> logger,
      Database db,
      IOptions<RACOptions> racOptions,
      RacRestClient httpClient) {
      _logger = logger;
      _db = db;
      _httpClient = httpClient;
      _racOptions = racOptions.Value;
      
      _logger.LogInformation("Base URL: {Url}", _racOptions.BaseUrl);
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      while (!stoppingToken.IsCancellationRequested) {
        _logger.LogInformation("FlightDataWorker running at: {Time}", DateTimeOffset.Now);
        // await SendFlightDataAsync(stoppingToken);
        await Task.Delay(_racOptions.SendingIntervalInMinutes.Minutes(), stoppingToken);
      }
    }

    private async Task SendFlightDataAsync(CancellationToken stoppingToken) {
      var sectorDate = DateTime.Now.ToString("yyyy-MM-dd");
      var dt = await GetFlightsDataAsync(sectorDate);
      var token = await GetAuthTokenAsync(stoppingToken);
      var flightsRows = dt.Rows.OfType<DataRow>();

      await flightsRows.Paginate(_racOptions.MaxFlightsPerMinute, async rows => {
        foreach (var dr in rows) {
          var request = CreateFlightRequest(dr, dt, token);
          await _httpClient.LogAndExecute(request, stoppingToken);
        }

        await Task.Delay(1.Minutes(), stoppingToken);
      });
    }


    private RestRequest CreateFlightRequest(DataRow dr, DataTable dt, RacToken token) {
      var transactionId = dr["Uniquekey"].ToString();
      DataTable dtTemp = dt.Clone();
      dtTemp.ImportRow(dr);
      dtTemp.Columns.Remove("Uniquekey");
      string jsonBody = SerializeFlightDataToJson(dtTemp);
      jsonBody = jsonBody.Replace("[", string.Empty).Replace("]", string.Empty);

      var request = new RestRequest(_racOptions.FlightInfoApiEndPoint, Method.POST);

      request.AddHeader("Authorization", "Bearer " + token.AuthToken);
      request.AddHeader("transaction-id", transactionId);
      request.AddHeader("Originator", "FLYNAS_API");
      request.AddHeader("Content-Type", "application/json");
      request.AddJsonBody(jsonBody);
      
      return request;
    }


    private string SerializeFlightDataToJson(DataTable dtTemp) {
      var listRows = new List<Dictionary<string, object>>();
      dtTemp.Rows.OfType<DataRow>().ForEach(row => {
        var props = new Dictionary<string, object>();
        dtTemp.Columns.OfType<DataColumn>().ForEach(col => { props.Add(col.ColumnName, row[col.Ordinal] is DBNull ? null : row[col.Ordinal]); });
        listRows.Add(props);
      });

      return JsonSerializer.Serialize(listRows);
    }

    private async Task<RacToken> GetAuthTokenAsync(CancellationToken stoppingToken) {
      if (RacToken != null && DateTime.Now.AddSeconds(-100) <= RacToken.ExpireIn) return RacToken;

      var request = new RestRequest(_racOptions.AuthApiEndPoint, Method.POST);
      request.AddParameter("application/x-www-form-urlencoded", BuildUrlForOauth2(), ParameterType.RequestBody);

      var response = await _httpClient.LogAndExecute(request, stoppingToken);

      var deserializedResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(response.Content);
      if (deserializedResponse == null)
        throw new InvalidOperationException("Failed to get auth token");

      RacToken = new RacToken(
        deserializedResponse["access_token"].ToString(),
        DateTime.Now.AddSeconds(Convert.ToInt32(deserializedResponse["expires_in"].ToString()))
      );

      return RacToken;
    }

    private string BuildUrlForOauth2() {
      StringBuilder strUrl = new StringBuilder();
      strUrl.AppendFormat("{0}={1}", "grant_type", "client_credentials");
      strUrl.AppendFormat("&{0}={1}", "client_id", _racOptions.AuthClientId);
      strUrl.AppendFormat("&{0}={1}", "client_secret", _racOptions.AuthClientSecret);
      return strUrl.ToString();
    }

    private async Task<DataTable> GetFlightsDataAsync(string sectorDate) {
      string queryFileName = "Sql\\RAC.sql";
      string queryFileFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, queryFileName);
      if (!File.Exists(queryFileFullPath)) {
        _logger.LogCritical("Query file is not found at the correct path, check RAC.sql file");
        throw new ArgumentException(nameof(queryFileName));
      }

      string query = await File.ReadAllTextAsync(queryFileFullPath);
      List<SqlParameter> queryParams = new List<SqlParameter>();
      queryParams.Add(new SqlParameter("@sectoredate", sectorDate));
      DataTable result = await _db.ExecuteQueryAsync(query, queryParams, false);
      return result;
    }
  }
}