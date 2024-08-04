using Microsoft.Extensions.Configuration;
using StravaClubStatsEngine.Service;
using StravaClubStatsEngine.Service.CosmosDb;
using StravaClubStatsShared.Models;

var builder = new ConfigurationBuilder()
                             .SetBasePath($"{Directory.GetCurrentDirectory()}/../../..")
                             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

var config = builder.Build();

Int32.TryParse(config["ClientID"], out int clientID);

Int32.TryParse(config["ClubID"], out int clubID);

Int32.TryParse(config["NumberOfPages"], out int numberOfPages);

var input = new StravaClubStatsEngineInput()
{
    StravaClubAPIUrl = config["StravaClubAPIUrl"],
    ClientID = clientID,
    ClientSecret = config["ClientSecret"],
    RefreshToken = config["RefreshToken"],
    ClubID = clubID,
    NumberOfPages = numberOfPages,
    CosmosDbEndointUrl = config["CosmosDbEndpointUrl"],
    CosmosDbPrimaryKey = config["CosmosDbPrimaryKey"],
    CosmosDatabase = config["CosmosDatabase"],
    CosmosPartitionKey = config["CosmosPartitionKey"],
};

var connection = new CosmosDbConnection(input);

var stravaClubStatsForYearService = new StravaClubStatsForYearService(connection);

var ride = new Ride
{
    Id = new Guid("f8fca7d5-3f3c-4682-b4d4-65f13c069571"),
    Cyclist = "John Tomlinson",
    Distance = 20,
    Time = "1 hr 50m",
    ElevationGain = 0.156M
};

bool success = await stravaClubStatsForYearService.UpdateStravaClubStatsForYearAsync(ride);

var clubStatsForCyclist = await stravaClubStatsForYearService.GetStravaClubStatsForCyclistAsync(ride.Id);

Console.WriteLine("Press any key to continue...");
Console.ReadLine();