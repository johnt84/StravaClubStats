using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StravaClubStatsEngine;
using StravaClubStatsEngine.Service;
using StravaClubStatsEngine.Service.API;
using StravaClubStatsEngine.Service.API.Interface;
using StravaClubStatsEngine.Service.CosmosDb;
using StravaClubStatsEngine.Service.CosmosDb.Interface;
using StravaClubStatsEngine.Service.Interface;
using StravaClubStatsShared.Models;
using StravaClubStatsSyncFunctionApp.Sync;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.ConfigureFunctionsWebApplication();

string GetRequiredConfiguration(string key) =>
    builder.Configuration[key] ?? throw new InvalidOperationException($"Missing configuration value '{key}'.");

var stravaClubStatsEngineInput = new StravaClubStatsEngineInput
{
    StravaClubAPIUrl = GetRequiredConfiguration("StravaClubAPIUrl"),
    ClientID = int.TryParse(builder.Configuration["ClientID"], out var clientId) ? clientId : 0,
    ClientSecret = GetRequiredConfiguration("ClientSecret"),
    RefreshToken = GetRequiredConfiguration("RefreshToken"),
    ClubID = int.TryParse(builder.Configuration["ClubID"], out var clubId) ? clubId : 0,
    NumberOfPages = int.TryParse(builder.Configuration["NumberOfPages"], out var numberOfPages) ? numberOfPages : 1,
    CosmosDbEndointUrl = GetRequiredConfiguration("CosmosDbEndpointUrl"),
    CosmosDbPrimaryKey = GetRequiredConfiguration("CosmosDbPrimaryKey"),
    CosmosDatabase = GetRequiredConfiguration("CosmosDatabase"),
    CosmosPartitionKey = GetRequiredConfiguration("CosmosPartitionKey"),
};

builder.Services.Configure<StravaAthleteSyncOptions>(builder.Configuration.GetSection(StravaAthleteSyncOptions.SectionName));
builder.Services.AddSingleton(stravaClubStatsEngineInput);
builder.Services.AddHttpClient<IHttpAPIClient, HttpAPIClient>();
builder.Services.AddSingleton<ICosmosDbConnection, CosmosDbConnection>();
builder.Services.AddSingleton<IStravaAthleteStatsService, StravaAthleteStatsService>();
builder.Services.AddSingleton<StravaClubStatsSyncService>();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
