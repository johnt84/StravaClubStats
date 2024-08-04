using StravaClubStatsShared.Models;
using StravaClubStatsShared.Models.FromAzure;

namespace StravaClubStatsEngine.Service.CosmosDb.Interface;

public interface ICosmosDbConnection
{
    Task<List<ClubStatsForYear>> QueryAsync();
    Task<ClubStatsForYear> GetCyclistStatsForYearAsync(Guid cyclistId);
    Task<bool> UpdateStravaClubStatsForYearAsync(StravaClubStatsForYear clubStatsForYear);
}
