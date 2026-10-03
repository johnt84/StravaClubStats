using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using StravaClubStatsEngine.Service.API.Interface;
using StravaClubStatsEngine.Service.Interface;
using StravaClubStatsShared.Models;
using StravaClubStatsShared.Models.FromAPI;

namespace StravaClubStatsEngine.Service;

public partial class StravaAthleteStatsService : IStravaAthleteStatsService
{
    private static readonly Regex AthleteNameRegex = new("\"firstName\":\"(?<first>[^\"]+)\",\"lastName\":\"(?<last>[^\"]+)\"", RegexOptions.Compiled);

    private readonly IHttpAPIClient _httpAPIClient;
    private readonly HttpClient _publicSiteHttpClient;
    private readonly StravaClubStatsEngineInput _stravaClubStatsEngineInput;

    public StravaAthleteStatsService(
        IHttpAPIClient httpAPIClient,
        StravaClubStatsEngineInput stravaClubStatsEngineInput,
        HttpClient? publicSiteHttpClient = null)
    {
        _httpAPIClient = httpAPIClient;
        _stravaClubStatsEngineInput = stravaClubStatsEngineInput;
        _publicSiteHttpClient = publicSiteHttpClient ?? CreatePublicSiteHttpClient();
    }

    public async Task<IReadOnlyList<StravaAthleteYearToDateCyclingStats>> GetCyclingYearToDateStatsAsync(
        IEnumerable<string> athleteProfileUrls,
        int targetYear,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(athleteProfileUrls);

        if (targetYear != DateTime.UtcNow.Year)
        {
            throw new InvalidOperationException($"Strava athlete stats sync only supports the current year totals. Configured year '{targetYear}' does not match '{DateTime.UtcNow.Year}'.");
        }

        var refreshApiToken = await GetRefreshApiTokenAsync();

        var results = new List<StravaAthleteYearToDateCyclingStats>();

        foreach (var athleteProfileUrl in athleteProfileUrls.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var athleteId = ParseAthleteId(athleteProfileUrl);
            var athleteName = await GetAthleteNameAsync(_publicSiteHttpClient, athleteId, cancellationToken);
            var athleteStats = await GetAthleteStatsAsync(refreshApiToken, athleteId);
            var rideTotals = athleteStats.ytd_ride_totals;

            results.Add(new StravaAthleteYearToDateCyclingStats
            {
                AthleteId = athleteId,
                AthleteName = athleteName,
                Activities = rideTotals.count,
                DistanceInKilometers = rideTotals.distance / 1000.00M,
                ElevationGainInMetres = rideTotals.elevation_gain,
                Time = FormatTime(rideTotals.moving_time),
                AthleteProfileUrl = athleteProfileUrl,
            });
        }

        return results;
    }

    private async Task<RefreshAPIToken> GetRefreshApiTokenAsync()
    {
        var response = await _httpAPIClient.PostAsync($"oauth/token?client_id={_stravaClubStatsEngineInput.ClientID}&client_secret={_stravaClubStatsEngineInput.ClientSecret}&grant_type=refresh_token&refresh_token={_stravaClubStatsEngineInput.RefreshToken}");

        return await response.Content.ReadFromJsonAsync<RefreshAPIToken>()
            ?? throw new InvalidOperationException("Unable to deserialize the Strava refresh token response.");
    }

    private async Task<StravaAthleteStats> GetAthleteStatsAsync(RefreshAPIToken refreshApiToken, int athleteId)
    {
        string json = await _httpAPIClient.GetAsync($"athletes/{athleteId}/stats?access_token={refreshApiToken.access_token}");

        return JsonConvert.DeserializeObject<StravaAthleteStats>(json)
            ?? throw new InvalidOperationException($"Unable to deserialize the Strava athlete stats response for athlete '{athleteId}'.");
    }

    private static HttpClient CreatePublicSiteHttpClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://www.strava.com/", UriKind.Absolute),
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

        return httpClient;
    }

    private static int ParseAthleteId(string athleteProfileUrl)
    {
        if (!Uri.TryCreate(athleteProfileUrl, UriKind.Absolute, out var athleteUri))
        {
            throw new InvalidOperationException($"The athlete profile URL '{athleteProfileUrl}' is not a valid absolute URI.");
        }

        var athleteIdSegment = athleteUri.Segments.LastOrDefault()?.Trim('/');

        if (!int.TryParse(athleteIdSegment, out var athleteId))
        {
            throw new InvalidOperationException($"The athlete profile URL '{athleteProfileUrl}' does not contain a valid athlete id.");
        }

        return athleteId;
    }

    private static async Task<string> GetAthleteNameAsync(HttpClient publicSiteClient, int athleteId, CancellationToken cancellationToken)
    {
        var html = await publicSiteClient.GetStringAsync($"athletes/{athleteId}", cancellationToken);
        var athleteNameMatch = AthleteNameRegex.Match(html);

        if (!athleteNameMatch.Success)
        {
            throw new InvalidOperationException($"Could not locate the athlete name in the Strava profile HTML for athlete '{athleteId}'.");
        }

        var firstName = WebUtility.HtmlDecode(Regex.Unescape(athleteNameMatch.Groups["first"].Value));
        var lastName = WebUtility.HtmlDecode(Regex.Unescape(athleteNameMatch.Groups["last"].Value));

        return string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string FormatTime(int movingTimeInSeconds)
    {
        var duration = TimeSpan.FromSeconds(movingTimeInSeconds);
        var totalHours = (int)duration.TotalHours;

        if (totalHours > 0 && duration.Minutes > 0)
        {
            return $"{totalHours}h {duration.Minutes}m";
        }

        if (totalHours > 0)
        {
            return $"{totalHours}h";
        }

        if (duration.Minutes > 0)
        {
            return $"{duration.Minutes}m";
        }

        return $"{Math.Max(duration.Seconds, 0)}s";
    }
}
