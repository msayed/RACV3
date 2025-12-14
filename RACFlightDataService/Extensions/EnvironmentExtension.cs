using Microsoft.Extensions.Hosting;

namespace RACFlightDataService.Extensions;

public static class EnvironmentExtension
{
    public static IHostBuilder SetEnv(this IHostBuilder builder, string[] args)
    {
        foreach (var arg in args)
        {
            if (arg == "-d")
                System.Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", Environments.Development);
            if (arg == "-s")
                System.Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", Environments.Staging);
            if (arg == "-p")
                System.Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", Environments.Production);
        }
        return builder;
    }
}