using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NSubstitute;
using StravaClubStatsEngine.Service;
using StravaClubStatsEngine.Service.API.Interface;
using StravaClubStatsShared.Models;
using StravaClubStatsShared.Models.FromAPI;

namespace StravaClubStatsEngineUnitTests.Service;

[TestFixture]
public class StravaClubStatsServiceTests
{
    [Test]
    public async Task GetClubActivitiesAsync_ShouldRefreshTokenFetchActivitiesAndMapResultsAsync()
    {
        var input = CreateInput();
        var refreshTokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new RefreshAPIToken
            {
                access_token = "access-token",
                refresh_token = "refresh-token",
                token_type = "Bearer",
            }),
        };

        var activitiesPayload = """
            [
              {
                "athlete": { "firstname": "Alex", "lastname": "Rider" },
                "name": "Lunch Ride",
                "distance": 12500,
                "moving_time": 5400,
                "elapsed_time": 6300,
                "total_elevation_gain": 450,
                "sport_type": "Ride"
              }
            ]
            """;

        var httpApiClient = Substitute.For<IHttpAPIClient>();
        httpApiClient.PostAsync(Arg.Any<string>()).Returns(refreshTokenResponse);
        httpApiClient.GetAsync(Arg.Any<string>()).Returns(activitiesPayload);

        var sut = new StravaClubStatsService(httpApiClient, input);

        var result = await sut.GetClubActivitiesAsync();

        result.Should().BeEquivalentTo(
            new[]
            {
                new Activity
                {
                    AthleteFirstName = "Alex",
                    AthleteLastName = "Rider",
                    ActivityName = "Lunch Ride",
                    SportType = "Ride",
                    DistanceInKilometers = 12.5m,
                    MovingTimeInHours = 1.5m,
                    ElapsedTimeInHours = 1.75m,
                    TotalElevationGainInKilometers = 0.45m,
                },
            });

        await httpApiClient.Received(1).PostAsync(
            "oauth/token?client_id=7&client_secret=secret&grant_type=refresh_token&refresh_token=refresh-token");
        await httpApiClient.Received(1).GetAsync(
            "clubs/99/activities?per_page=200&access_token=access-token");
    }

    [Test]
    public async Task GetClubActivitiesSummariesAsync_ShouldAggregateActivitiesPerAthleteAsync()
    {
        var input = CreateInput();
        var refreshTokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new RefreshAPIToken
            {
                access_token = "access-token",
                refresh_token = "refresh-token",
                token_type = "Bearer",
            }),
        };

        var activitiesPayload = """
            [
              {
                "athlete": { "firstname": "Alex", "lastname": "Rider" },
                "name": "Lunch Ride",
                "distance": 10000,
                "moving_time": 3600,
                "elapsed_time": 4200,
                "total_elevation_gain": 500,
                "sport_type": "Ride"
              },
              {
                "athlete": { "firstname": "Alex", "lastname": "Rider" },
                "name": "Evening Ride",
                "distance": 20000,
                "moving_time": 5400,
                "elapsed_time": 6000,
                "total_elevation_gain": 800,
                "sport_type": "Ride"
              },
              {
                "athlete": { "firstname": "Jamie", "lastname": "Climber" },
                "name": "Hill Session",
                "distance": 5000,
                "moving_time": 1800,
                "elapsed_time": 2100,
                "total_elevation_gain": 200,
                "sport_type": "Ride"
              }
            ]
            """;

        var httpApiClient = Substitute.For<IHttpAPIClient>();
        httpApiClient.PostAsync(Arg.Any<string>()).Returns(refreshTokenResponse);
        httpApiClient.GetAsync(Arg.Any<string>()).Returns(activitiesPayload);

        var sut = new StravaClubStatsService(httpApiClient, input);

        var result = await sut.GetClubActivitiesSummariesAsync();

        result.Should().BeEquivalentTo(
            new[]
            {
                new ActivitiesSummary
                {
                    AthleteFirstName = "Alex",
                    AthleteLastName = "Rider",
                    TotalNumberOfRides = 2,
                    TotalDistanceInKilometers = 30m,
                    LongestRideInKilometers = 20m,
                    TotalMovingTimeInHours = 2.5m,
                    TotalElapsedTimeInHours = (4200m / 60m / 60m) + (6000m / 60m / 60m),
                    TotalElevationGainInKilometers = 1.3m,
                    AverageDistancePerRideInKilometers = 15m,
                    AverageMovingTimeInHours = 1.25m,
                    AverageElapsedTimeInHours = ((4200m / 60m / 60m) + (6000m / 60m / 60m)) / 2m,
                    AverageElevationGainInKilometers = 0.65m,
                },
                new ActivitiesSummary
                {
                    AthleteFirstName = "Jamie",
                    AthleteLastName = "Climber",
                    TotalNumberOfRides = 1,
                    TotalDistanceInKilometers = 5m,
                    LongestRideInKilometers = 5m,
                    TotalMovingTimeInHours = 0.5m,
                    TotalElapsedTimeInHours = 2100m / 60m / 60m,
                    TotalElevationGainInKilometers = 0.2m,
                    AverageDistancePerRideInKilometers = 5m,
                    AverageMovingTimeInHours = 0.5m,
                    AverageElapsedTimeInHours = 2100m / 60m / 60m,
                    AverageElevationGainInKilometers = 0.2m,
                },
            },
            options => options.WithoutStrictOrdering());
    }

    private static StravaClubStatsEngineInput CreateInput() => new()
    {
        ClientID = 7,
        ClientSecret = "secret",
        RefreshToken = "refresh-token",
        ClubID = 99,
    };
}
