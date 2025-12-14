using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RACFlightDataService.Functions.HttpClients;
using RACFlightDataService.Functions.Options;

namespace RACFlightDataService.Functions.Services;

public interface IFlightDataJobService
{
    Task RunAsync(Guid runId, CancellationToken cancellationToken);
}

public class FlightDataJobService : IFlightDataJobService
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private readonly RacApiClient _client;
    private readonly ILogger<FlightDataJobService> _logger;
    private readonly JobOptions _jobOptions;

    public FlightDataJobService(
        RacApiClient client,
        IOptions<JobOptions> jobOptions,
        ILogger<FlightDataJobService> logger)
    {
        _client = client;
        _logger = logger;
        _jobOptions = jobOptions.Value;
    }

    public async Task RunAsync(Guid runId, CancellationToken cancellationToken)
    {
        var startUtc = DateTimeOffset.UtcNow;
        _logger.LogInformation("Starting job at {StartUtc}. RunId: {RunId}", startUtc, runId);

        if (!_jobOptions.Enabled)
        {
            _logger.LogWarning("Job disabled via configuration. RunId: {RunId}", runId);
            return;
        }

        var acquired = await Semaphore.WaitAsync(TimeSpan.Zero, cancellationToken);
        if (!acquired)
        {
            _logger.LogWarning(
                "Job already running. This lock is per-instance and not distributed. RunId: {RunId}",
                runId);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _client.SendAsync(runId, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "RAC API returned non-success status. StatusCode: {StatusCode}, RunId: {RunId}",
                    (int)response.StatusCode,
                    runId);
            }
            else
            {
                _logger.LogInformation(
                    "RAC API call succeeded. StatusCode: {StatusCode}, RunId: {RunId}",
                    (int)response.StatusCode,
                    runId);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Job cancelled. RunId: {RunId}", runId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job failed. RunId: {RunId}", runId);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            Semaphore.Release();
            _logger.LogInformation(
                "Job finished at {EndUtc} after {DurationMs} ms. RunId: {RunId}",
                DateTimeOffset.UtcNow,
                stopwatch.ElapsedMilliseconds,
                runId);
        }
    }
}
