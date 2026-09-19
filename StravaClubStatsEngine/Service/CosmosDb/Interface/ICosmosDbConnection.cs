using StravaClubStatsShared.Models.FromAzure;

namespace StravaClubStatsEngine.Service.CosmosDb.Interface;

public interface ICosmosDbConnection
{
    Task<List<ClubStatsForYear>> QueryAsync();

    Task UpsertAsync(ClubStatsForYear clubStatsForYear);
}
