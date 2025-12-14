# RACFlightDataService.Functions

Azure Functions timer job that migrates the RAC flight data scheduled worker to .NET 8 isolated worker.

## Configuration

Settings are loaded from `appsettings.json`, `local.settings.json`, and environment variables. Required keys:

- `JobSchedule` (cron expression with seconds)
- `Job:Enabled`
- `Api:BaseUrl`
- `Api:Endpoint`
- `Api:TimeoutSeconds`
- `Api:ApiKey` (optional)
- `Api:DefaultHeaders` (optional)

The default `local.settings.json` is configured to use Azurite:

```json
{
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

## Running locally

1. Ensure Azurite is running. The storage connection uses `UseDevelopmentStorage=true` by default.
2. Restore and build the solution: `dotnet build`.
3. Start the function host from the project directory: `func start` or `dotnet run`.
4. Update `local.settings.json` with real API values before calling external services.

## Common issues

- **Configuration validation failures**: the host stops at startup if required settings are missing or invalid. Confirm all keys are present and correctly typed.
- **Overlapping executions**: a `SemaphoreSlim` prevents concurrent runs within the same host instance only; it does not coordinate across scaled-out instances.
- **Network timeouts**: the HTTP client retries transient failures with exponential backoff. Increase `Api:TimeoutSeconds` if needed.
