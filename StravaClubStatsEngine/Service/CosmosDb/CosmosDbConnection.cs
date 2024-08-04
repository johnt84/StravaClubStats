using Microsoft.Azure.Cosmos;
using StravaClubStatsEngine.Service.CosmosDb.Interface;
using StravaClubStatsShared.Models;
using StravaClubStatsShared.Models.FromAzure;
using System.Net;
using Container = Microsoft.Azure.Cosmos.Container;

namespace StravaClubStatsEngine.Service.CosmosDb;

public class CosmosDbConnection : ICosmosDbConnection
{
    private readonly StravaClubStatsEngineInput _stravaClubStatsEngineInput;

    public CosmosDbConnection(StravaClubStatsEngineInput stravaClubStatsEngineInput)
    {
        _stravaClubStatsEngineInput = stravaClubStatsEngineInput;
    }

    public async Task<List<ClubStatsForYear>> QueryAsync()
    {
        var container = await SetUpAsync();

        var sqlQueryText = $"SELECT * FROM c";

        var queryDefinition = new QueryDefinition(sqlQueryText);
        var queryResultSetIterator = container.GetItemQueryIterator<ClubStatsForYear>(queryDefinition);

        var clubStatsForYear = new List<ClubStatsForYear>();

        while (queryResultSetIterator.HasMoreResults)
        {
            var response = await queryResultSetIterator.ReadNextAsync();

            foreach (var item in response)
            {
                clubStatsForYear.Add(item);
            }
        }

        return clubStatsForYear;
    }

    public async Task<ClubStatsForYear> GetCyclistStatsForYearAsync(Guid cyclistId)
    {
        var container = await SetUpAsync();

        return await container.ReadItemAsync<ClubStatsForYear>(cyclistId.ToString(), new PartitionKey(cyclistId.ToString()));
    }

    public async Task<bool> UpdateStravaClubStatsForYearAsync(StravaClubStatsForYear clubStatsForYear)
    {
        const string Km = "km";
        const string M = "m";

        var container = await SetUpAsync();

        var partitionKey = new PartitionKey(clubStatsForYear.Id.ToString());

        var clubStatsForYearToUpdate = new ClubStatsForYear
        {
            id = clubStatsForYear.Id.ToString(),
            cyclist = clubStatsForYear.Cyclist,
            rides = clubStatsForYear.Rides.ToString(),
            time = clubStatsForYear.Time,
            distance = $"{clubStatsForYear.Distance.ToString()} {Km}",
            elevationgain = $"{clubStatsForYear.ElevationGain.ToString()} {M}",
            distancetarget = $"{clubStatsForYear.DistanceTarget.ToString()} {Km}"
        };

        var itemResponse = await container.ReplaceItemAsync<ClubStatsForYear>(clubStatsForYearToUpdate, clubStatsForYear.Id.ToString(), partitionKey);

        return itemResponse.StatusCode == HttpStatusCode.OK;
    }

    private async Task<Container> SetUpAsync()
    {
        var cosmosClient = new CosmosClient(_stravaClubStatsEngineInput.CosmosDbEndointUrl, _stravaClubStatsEngineInput.CosmosDbPrimaryKey);

        Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(_stravaClubStatsEngineInput.CosmosDatabase);

        return await database.CreateContainerIfNotExistsAsync(_stravaClubStatsEngineInput.CosmosPartitionKey, "/id");
    }
}
