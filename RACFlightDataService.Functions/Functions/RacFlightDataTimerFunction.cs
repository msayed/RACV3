using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RACFlightDataService.Functions.Options;
using RACFlightDataService.Functions.Services;

namespace RACFlightDataService.Functions.Functions;

public class RacFlightDataTimerFunction
{
    private readonly IFlightDataJobService _jobService;
    private readonly ILogger<RacFlightDataTimerFunction> _logger;

    public RacFlightDataTimerFunction(
        IFlightDataJobService jobService,
        IOptions<JobOptions> jobOptions,
        ILogger<RacFlightDataTimerFunction> logger)
    {
        _jobService = jobService;
        _logger = logger;
        _ = jobOptions.Value;
    }

    [Function("RacFlightDataTimerFunction")]
    public async Task RunAsync([TimerTrigger("%JobSchedule%", RunOnStartup = true)] TimerInfo timerInfo, CancellationToken cancellationToken)
    {
        var runId = Guid.NewGuid();
        var startUtc = DateTimeOffset.UtcNow;

        _logger.LogInformation(
            "Timer triggered at {StartUtc}. IsPastDue: {IsPastDue}. Next: {NextOccurrences}. RunId: {RunId}",
            startUtc,
            timerInfo.IsPastDue,
            timerInfo.ScheduleStatus?.Next,
            runId);

        try
        {
            await _jobService.RunAsync(runId, cancellationToken);
            _logger.LogInformation(
                "Timer job succeeded. Next: {NextOccurrences}. RunId: {RunId}",
                timerInfo.ScheduleStatus?.Next,
                runId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Timer job cancelled. RunId: {RunId}", runId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Timer job failed. RunId: {RunId}", runId);
            throw;
        }
        finally
        {
            var duration = DateTimeOffset.UtcNow - startUtc;
            _logger.LogInformation(
                "Timer execution finished. DurationMs: {DurationMs}. RunId: {RunId}",
                duration.TotalMilliseconds,
                runId);
        }
    }
}
