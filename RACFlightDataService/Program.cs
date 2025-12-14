using System;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;


// try
// {
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, configuration) =>
    {
        var env = context.HostingEnvironment.EnvironmentName;
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true)
            .AddJsonFile($"appsettings.{env}.json", true)
            .AddEnvironmentVariables()
            .Build();
        configuration
            .ReadFrom.Configuration(config)
            .Enrich.FromLogContext();
        Log.Logger.Information("current env is {env}", env);
    });
    builder.Services.AddAppServices(builder.Configuration, builder.Environment);
    var app = builder.Build();
    app.UseStaticFiles();
    
    app.UseHangfireDashboard("/hangfire",new DashboardOptions()
    {
        Authorization = new[] { new DashboardNoAuthorizationFilter() }
    });
    app.MapGet("/", () => "rac flight data api started!");

    Log.Logger.Information("Starting up");
    app.Run();
    Log.Logger.Information("Service stopped");
// }
// catch (Exception e )
// {
//     Log.Logger.Error(e,"Failed to start service");
// }