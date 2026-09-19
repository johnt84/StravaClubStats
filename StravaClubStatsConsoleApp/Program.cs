using StravaClubStatsShared.Models;
using StravaClubStatsEngine.Service;
using StravaClubStatsEngine.Service.API;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
                               .SetBasePath($"{Directory.GetCurrentDirectory()}/../../..")
                               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

var config = builder.Build();

string GetRequiredConfiguration(string key) =>
    config[key] ?? throw new InvalidOperationException($"Missing configuration value '{key}'.");

Int32.TryParse(config["ClientID"], out int clientID);

Int32.TryParse(config["ClubID"], out int clubID);

Int32.TryParse(config["NumberOfPages"], out int numberOfPages);

var stravaClubStatsEngineInput = new StravaClubStatsEngineInput()
{
    StravaClubAPIUrl = GetRequiredConfiguration("StravaClubAPIUrl"),
    ClientID = clientID,
    ClientSecret = GetRequiredConfiguration("ClientSecret"),
    RefreshToken = GetRequiredConfiguration("RefreshToken"),
    ClubID = clubID,
    NumberOfPages = numberOfPages,
};

var httpClient = new HttpClient();

httpClient.BaseAddress = new Uri(stravaClubStatsEngineInput.StravaClubAPIUrl, UriKind.Absolute);

var httpAPIClient = new HttpAPIClient(httpClient, stravaClubStatsEngineInput);

var stravaClubStatsService = new StravaClubStatsService(httpAPIClient, stravaClubStatsEngineInput);

var clubActivitiesSummaries = await stravaClubStatsService.GetClubActivitiesSummariesAsync();

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
