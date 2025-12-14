using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using RACFlightDataService.Functions.HttpClients;
using RACFlightDataService.Functions.Options;

namespace RACFlightDataService.Functions.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JobOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .Validate(options => configuration.GetSection("Job").GetValue<string>("Enabled") is not null,
                "Job:Enabled is required")
            .ValidateOnStart();

        services.AddOptionsWithValidation<ApiOptions>(configuration, "Api");

        services.AddHttpClient<RacApiClient>((provider, client) =>
            {
                var apiOptions = provider.GetRequiredService<IOptions<ApiOptions>>().Value;
                client.BaseAddress = new Uri(apiOptions.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(apiOptions.TimeoutSeconds);
            })
            .AddPolicyHandler(GetRetryPolicy());

        services.AddSingleton<IFlightDataJobService, Services.FlightDataJobService>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static void AddOptionsWithValidation<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName) where TOptions : class
    {
        var section = string.IsNullOrWhiteSpace(sectionName)
            ? configuration
            : configuration.GetSection(sectionName);

        services.AddOptions<TOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
