using StravaClubStatsShared.Models;

namespace StravaClubStatsEngine.Service.Interface;

public interface IStravaClubStatsForYearService
{
    Task<List<StravaClubStatsForYear>> GetStravaClubStatsForYearAsync();
    Task<StravaClubStatsForYear?> GetStravaClubStatsForCyclistAsync(Guid cyclistId);
    Task<bool> UpdateStravaClubStatsForYearAsync(Ride ride);
}
