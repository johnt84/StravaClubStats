using System.Globalization;
using System.Text.RegularExpressions;
using StravaClubStatsEngine.Service.CosmosDb.Interface;
using StravaClubStatsEngine.Service.Interface;

namespace StravaClubStatsEngine;

public class StravaClubStatsSyncService
{
    private const string KilometersSuffix = " km";
    private const string MetresSuffix = " m";
    private static readonly Regex MultipleWhitespaceRegex = new("\\s+", RegexOptions.Compiled);

    private readonly ICosmosDbConnection _cosmosDbConnection;
    private readonly IStravaAthleteStatsService _stravaAthleteStatsService;

    public StravaClubStatsSyncService(
        ICosmosDbConnection cosmosDbConnection,
        IStravaAthleteStatsService stravaAthleteStatsService)
    {
        _cosmosDbConnection = cosmosDbConnection;
        _stravaAthleteStatsService = stravaAthleteStatsService;
    }

    public async Task<int> SyncAsync(IEnumerable<string> athleteProfileUrls, int targetYear, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(athleteProfileUrls);

        var athleteStats = await _stravaAthleteStatsService
            .GetCyclingYearToDateStatsAsync(athleteProfileUrls, targetYear, cancellationToken);

        var existingRecords = await _cosmosDbConnection.QueryAsync();
        var recordsByCyclistName = existingRecords
            .GroupBy(record => NormalizeCyclistName(record.cyclist))
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        var updatedAthleteCount = 0;

        foreach (var athlete in athleteStats)
        {
            if (!recordsByCyclistName.TryGetValue(NormalizeCyclistName(athlete.AthleteName), out var existingRecord))
            {
                continue;
            }

            existingRecord.rides = athlete.Activities.ToString(CultureInfo.InvariantCulture);
            existingRecord.time = athlete.Time;
            existingRecord.distance = FormatDistance(athlete.DistanceInKilometers);
            existingRecord.elevationgain = FormatElevationGain(athlete.ElevationGainInMetres);
            existingRecord.cyclist = string.IsNullOrWhiteSpace(existingRecord.cyclist)
                ? athlete.AthleteName
                : existingRecord.cyclist;

            await _cosmosDbConnection.UpsertAsync(existingRecord);
            updatedAthleteCount++;
        }

        return updatedAthleteCount;
    }

    private static string NormalizeCyclistName(string cyclistName) =>
        MultipleWhitespaceRegex.Replace(cyclistName, " ").Trim().ToUpperInvariant();

    private static string FormatDistance(decimal value) =>
        string.Create(CultureInfo.InvariantCulture, $"{value:0.##}{KilometersSuffix}");

    private static string FormatElevationGain(decimal value) =>
        string.Create(CultureInfo.InvariantCulture, $"{value:0.##}{MetresSuffix}");
}
