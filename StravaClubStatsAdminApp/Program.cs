using MudBlazor.Services;
using StravaClubStatsAdminApp.Components;
using StravaClubStatsEngine.Service.CosmosDb;
using StravaClubStatsEngine.Service.CosmosDb.Interface;
using StravaClubStatsShared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

string GetRequiredConfiguration(string key) =>
    builder.Configuration[key] ?? throw new InvalidOperationException($"Missing configuration value '{key}'.");

var stravaClubStatsEngineInput = new StravaClubStatsEngineInput
{
    StravaClubAPIUrl = builder.Configuration["StravaClubAPIUrl"] ?? string.Empty,
    ClientID = int.TryParse(builder.Configuration["ClientID"], out var clientId) ? clientId : 0,
    ClientSecret = builder.Configuration["ClientSecret"] ?? string.Empty,
    RefreshToken = builder.Configuration["RefreshToken"] ?? string.Empty,
    ClubID = int.TryParse(builder.Configuration["ClubID"], out var clubId) ? clubId : 0,
    NumberOfPages = int.TryParse(builder.Configuration["NumberOfPages"], out var numberOfPages) ? numberOfPages : 1,
    CosmosDbEndointUrl = GetRequiredConfiguration("CosmosDbEndpointUrl"),
    CosmosDbPrimaryKey = GetRequiredConfiguration("CosmosDbPrimaryKey"),
    CosmosDatabase = GetRequiredConfiguration("CosmosDatabase"),
    CosmosPartitionKey = GetRequiredConfiguration("CosmosPartitionKey"),
};

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddSingleton(stravaClubStatsEngineInput);
builder.Services.AddSingleton<ICosmosDbConnection, CosmosDbConnection>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
