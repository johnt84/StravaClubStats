using Newtonsoft.Json;
using StravaClubStatsShared.Models.FromAPI;
using StravaClubStatsEngine.Service.API.Interface;
using StravaClubStatsEngine.Service.Interface;
using StravaClubStatsShared.Models;
using System.Net.Http.Json;

namespace StravaClubStatsEngine.Service;

public class StravaClubStatsService : IStravaClubStatsService
{
    private readonly IHttpAPIClient _httpAPIClient;
    private readonly StravaClubStatsEngineInput _stravaClubStatsEngineInput;

    public StravaClubStatsService(IHttpAPIClient httpAPIClient, StravaClubStatsEngineInput stravaClubStatsEngineInput)
    {
        _httpAPIClient = httpAPIClient;
        _stravaClubStatsEngineInput = stravaClubStatsEngineInput;
    }

    public async Task<List<ActivitiesSummary>> GetClubActivitiesSummariesAsync()
    {
        var clubActvities = await GetClubActivitiesAsync();

        return GetActivitiesSummary(clubActvities);
    }

    public async Task<List<Activity>> GetClubActivitiesAsync()
    {
        var refreshAPIToken = await GetRefreshAPITokenAsync();

        var stravaClubActivitiesFromAPI = await GetStravaClubActivitiesFromAPIAsync(refreshAPIToken);

        return ConvertFromClubActvitiesFromAPIToClubActvities(stravaClubActivitiesFromAPI);
    }

    private List<Activity> ConvertFromClubActvitiesFromAPIToClubActvities(List<StravaClubActivities> stravaClubActivitiesFromAPI)
    {
        return stravaClubActivitiesFromAPI
                            .Select(x =>
                                        new Activity()
                                        {
                                            AthleteFirstName = x.athlete.firstname,
                                            AthleteLastName = x.athlete.lastname,
                                            ActivityName = x.name,
                                            SportType = x.sport_type,
                                            DistanceInKilometers = ((decimal)x.distance) / 1000.00M,
                                            MovingTimeInHours = ((decimal)x.moving_time) / 60.00M / 60.00M,
                                            ElapsedTimeInHours = ((decimal)x.elapsed_time) / 60.00M / 60.00M,
                                            TotalElevationGainInKilometers = ((decimal)x.total_elevation_gain) / 1000.00M,
                                        })
                                .ToList();
    }

    private List<ActivitiesSummary> GetActivitiesSummary(List<Activity> clubActvities)
    {
        return clubActvities
                    .GroupBy(x =>
                                new
                                {
                                    x.AthleteFirstName,
                                    x.AthleteLastName
                                })
                    .Select(x => new ActivitiesSummary()
                    {
                        AthleteFirstName = x.Key.AthleteFirstName,
                        AthleteLastName = x.Key.AthleteLastName,
                        TotalNumberOfRides = x.Count(),
                        TotalDistanceInKilometers = x.Sum(x => x.DistanceInKilometers),
                        LongestRideInKilometers = x.Max(x => x.DistanceInKilometers),
                        TotalMovingTimeInHours = x.Sum(x => x.MovingTimeInHours),
                        TotalElapsedTimeInHours = x.Sum(x => x.ElapsedTimeInHours),
                        TotalElevationGainInKilometers = x.Sum(x => x.TotalElevationGainInKilometers),
                        AverageDistancePerRideInKilometers = x.Sum(x => x.DistanceInKilometers) / x.Count(),
                        AverageMovingTimeInHours = x.Sum(x => x.MovingTimeInHours) / x.Count(),
                        AverageElapsedTimeInHours = x.Sum(x => x.ElapsedTimeInHours) / x.Count(),
                        AverageElevationGainInKilometers = x.Sum(x => x.TotalElevationGainInKilometers) / x.Count(),
                    })
                    .ToList();
    }

    private async Task<RefreshAPIToken> GetRefreshAPITokenAsync()
    {
        var response = await _httpAPIClient.PostAsync($"oauth/token?client_id={_stravaClubStatsEngineInput.ClientID}&client_secret={_stravaClubStatsEngineInput.ClientSecret}&grant_type=refresh_token&refresh_token={_stravaClubStatsEngineInput.RefreshToken}");
        return await response.Content.ReadFromJsonAsync<RefreshAPIToken>()
            ?? throw new InvalidOperationException("Unable to deserialize the Strava refresh token response.");
    }

    private async Task<List<StravaClubActivities>> GetStravaClubActivitiesFromAPIAsync(RefreshAPIToken refreshAPIToken)
    {
        string json = await _httpAPIClient.GetAsync($"clubs/{_stravaClubStatsEngineInput.ClubID}/activities?per_page=200&access_token={refreshAPIToken.access_token}");
        return JsonConvert.DeserializeObject<List<StravaClubActivities>>(json)
            ?? throw new InvalidOperationException("Unable to deserialize the Strava club activities response.");
    }
}