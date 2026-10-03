using System.Net;
using System.Net.Http;
using FluentAssertions;
using NSubstitute;
using StravaClubStatsEngine.Service;
using StravaClubStatsEngine.Service.API.Interface;
using StravaClubStatsShared.Models;

namespace StravaClubStatsEngineUnitTests.Service;

[TestFixture]
public class StravaAthleteStatsServiceTests
{
    [Test]
    public async Task GetCyclingYearToDateStatsAsync_ShouldMapAthleteNamesAndYearToDateRideTotalsAsync()
    {
        var apiClient = Substitute.For<IHttpAPIClient>();
        apiClient.PostAsync(Arg.Any<string>()).Returns(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"access_token\":\"token-123\"}"),
        });
        apiClient.GetAsync("athletes/32760466/stats?access_token=token-123").Returns("{\"ytd_ride_totals\":{\"count\":42,\"distance\":123456.0,\"moving_time\":3665,\"elevation_gain\":5432.0}}");

        var publicSiteHandler = new StubHttpMessageHandler
        {
            ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<html><body><script>{\"firstName\":\"John\",\"lastName\":\"Tomlinson\"}</script></body></html>"),
            },
        };

        var publicSiteClient = new HttpClient(publicSiteHandler)
        {
            BaseAddress = new Uri("https://www.strava.com/", UriKind.Absolute),
        };

        var sut = new StravaAthleteStatsService(apiClient, CreateInput(), publicSiteClient);

        var result = await sut.GetCyclingYearToDateStatsAsync(["https://www.strava.com/athletes/32760466"], DateTime.UtcNow.Year);

        result.Should().ContainSingle();
        result[0].Should().BeEquivalentTo(new
        {
            AthleteId = 32760466,
            AthleteName = "John Tomlinson",
            Activities = 42,
            DistanceInKilometers = 123.456m,
            ElevationGainInMetres = 5432m,
            Time = "1h 1m",
            AthleteProfileUrl = "https://www.strava.com/athletes/32760466",
        });
    }

    [Test]
    public async Task GetCyclingYearToDateStatsAsync_ShouldThrowWhenTargetYearIsNotTheCurrentYearAsync()
    {
        var apiClient = Substitute.For<IHttpAPIClient>();
        var sut = new StravaAthleteStatsService(apiClient, CreateInput());

        var action = async () => await sut.GetCyclingYearToDateStatsAsync(["https://www.strava.com/athletes/32760466"], DateTime.UtcNow.Year - 1);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*only supports the current year totals*");
    }

    private static StravaClubStatsEngineInput CreateInput() => new()
    {
        StravaClubAPIUrl = "https://www.strava.com/api/v3/",
        ClientID = 123,
        ClientSecret = "secret",
        RefreshToken = "refresh-token",
    };

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        public required Func<HttpRequestMessage, HttpResponseMessage> ResponseFactory { get; init; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(ResponseFactory(request));
    }
}
