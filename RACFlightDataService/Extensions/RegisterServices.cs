using System;
using Hangfire;
using Hangfire.MAMQSqlExtension;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ORMContext;
using RACFlightDataService.Databases.Gaca;
using RACFlightDataService.Extensions;
using RACFlightDataService.HttpClients.Rac;
using RACFlightDataService.Jobs;
using RACFlightDataService.Logging;
using RACFlightDataService.Options;
using RACFlightDataService.Service;

public static class RegisterServices
{
    public static void AddAppServices(this IServiceCollection serviceCollection, IConfiguration configuration,
        IHostEnvironment env)
    {
        serviceCollection
            .AddTransient(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>))
            .Configure<RACOptions>(configuration.GetSection("RAC"))
            .AddTransient<RacRestClient>()
            .AddTransient<TavRestClient>()
            .AddSingleton<IRacAuth,RacAuth>()
            .AddHangfire(x => 
                x.UseMAMQSqlServerStorage(configuration.GetConnectionString("HangFireConnection"), new SqlServerStorageOptions()
                {
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true,
                },new[] { "rac-flight" })
            )
            .AddHangfireServer(( options) => 
            {
               options.ServerName = string.Format("{0}:rac-flight", Environment.MachineName);
               options.Queues = new[] { "rac-flight" };
               options.WorkerCount = 3;
            })
            
            .AddHostedService<FlightsJobService>()
            .AddScoped<IRacService, RacService>()
            .AddScoped<IGacaDbFactory, GacaDbFactory>()
            .AddScoped<IGacaRepository, GacaRepository>()
            .AddMapster()
            .RegisterDb(configuration)
            ;
    }

    public static IServiceCollection RegisterDb(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("RacDbConnection");
        // services.AddDbContext<AppDbContext>();
        // services.AddDbContextFactory<AppDbContext>(opt => opt.UseSqlServer(connection));
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connection));
        services.AddScoped<IAppDbContext>(s=>s.GetService<AppDbContext>());
        return services;
    }
}