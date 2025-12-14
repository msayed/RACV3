using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RACFlightDataService.Functions.Options;

namespace RACFlightDataService.Functions.HttpClients;

public class RacApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RacApiClient> _logger;
    private readonly ApiOptions _options;

    public RacApiClient(HttpClient httpClient, IOptions<ApiOptions> options, ILogger<RacApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<HttpResponseMessage> SendAsync(Guid runId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, _options.Endpoint);
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        request.Headers.Add("Idempotency-Key", runId.ToString());
        if (_options.DefaultHeaders is not null)
        {
            foreach (var header in _options.DefaultHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        var stopwatch = Stopwatch.StartNew();
        var response = await _httpClient.SendAsync(request, cancellationToken);
        stopwatch.Stop();

        var content = response.Content is null
            ? string.Empty
            : await response.Content.ReadAsStringAsync(cancellationToken);

        var truncated = content.Length > 500 ? content[..500] : content;

        _logger.LogInformation(
            "Completed RAC API call. Url: {Url}, StatusCode: {StatusCode}, DurationMs: {Duration}, Response: {Response}",
            request.RequestUri,
            (int)response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            truncated);

        return response;
    }
}
