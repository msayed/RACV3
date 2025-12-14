using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NJsonSchema.Infrastructure;
using ORMContext;
using ORMContext.Domain;
using RACFlightDataService.Databases.Gaca;
using RACFlightDataService.Databases.Gaca.Models;
using RACFlightDataService.Extensions;
using RACFlightDataService.HttpClients.Rac;
using RACFlightDataService.HttpClients.Rac.Requests;
using RACFlightDataService.Logging;
using RACFlightDataService.Options;
using RACFlightDataService.Service;
using RestSharp;

namespace RACFlightDataService.Jobs;

public class FlightsJobService : IHostedService
{
    private readonly ILoggerAdapter<FlightsJobService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private IMonitoringApi _monitoringApi;

    public FlightsJobService(ILoggerAdapter<FlightsJobService> logger,
        IServiceScopeFactory serviceScopeFactory,IBackgroundJobClient backgroundJobClient,IRecurringJobManager recurringJobManager)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _logger.SetServiceName(this.GetType().Name);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetService<IAppDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
        var racService = scope.ServiceProvider.GetService<IRacService>();
        _logger.LogInformation("Starting create jobs for the first time ...");

        // _monitoringApi.PurgeJobs();
        // var jobs = _monitoringApi.EnqueuedJobs("rac-flight");
        // // var jobs = _monitoringApi.ProcessingJobs( 0, 10);
        // foreach (var job in jobs.Where(j=>j.Value.))
        // {
        //     BackgroundJob.Delete(job.Key);
        // }

        _recurringJobManager.AddOrUpdate("tav-auto-service-refreshing",
         () => racService.TavAutoServiceProcess(cancellationToken), "*/10 * * * *", queue: "rac-flight");

        _recurringJobManager.AddOrUpdate("pull-from-gaca", 
            ()=> racService.PullFlightsFromGacaAsync(cancellationToken),"*/10 * * * *",queue:"rac-flight");
        _recurringJobManager.AddOrUpdate("notify-rac", 
            ()=> racService.SentForRuh(cancellationToken),"*/5 * * * *",queue:"rac-flight");
     
    }

  

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }
}