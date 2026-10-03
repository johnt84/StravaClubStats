using StravaClubStatsShared.Models;

namespace StravaClubStatsEngine.Service.Interface;

public interface IStravaAthleteStatsService
{
    Task<IReadOnlyList<StravaAthleteYearToDateCyclingStats>> GetCyclingYearToDateStatsAsync(IEnumerable<string> athleteProfileUrls, int targetYear, CancellationToken cancellationToken = default);
}
