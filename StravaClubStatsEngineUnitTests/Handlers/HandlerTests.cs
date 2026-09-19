using FluentAssertions;
using NSubstitute;
using StravaClubStatsEngine.Handlers;
using StravaClubStatsEngine.Queries;
using StravaClubStatsEngine.Service.Interface;
using StravaClubStatsShared.Models;

namespace StravaClubStatsEngineUnitTests.Handlers;

[TestFixture]
public class GetClubActivitiesHandlerTests
{
    [Test]
    public async Task Handle_ShouldReturnActivitiesFromServiceAsync()
    {
        var expectedActivities = new List<Activity>
        {
            new()
            {
                AthleteFirstName = "Sam",
                AthleteLastName = "Hill",
                ActivityName = "Morning Ride",
                SportType = "Ride",
                DistanceInKilometers = 42.5m,
            },
        };

        var service = Substitute.For<IStravaClubStatsService>();
        service.GetClubActivitiesAsync().Returns(expectedActivities);

        var sut = new GetClubActivitiesHandler(service);

        var result = await sut.Handle(new GetClubActivitiesQuery(), CancellationToken.None);

        result.Should().BeSameAs(expectedActivities);
        await service.Received(1).GetClubActivitiesAsync();
    }
}

[TestFixture]
public class GetClubActivitiesSummariesHandlerTests
{
    [Test]
    public async Task Handle_ShouldReturnActivitySummariesFromServiceAsync()
    {
        var expectedSummaries = new List<ActivitiesSummary>
        {
            new()
            {
                AthleteFirstName = "Taylor",
                AthleteLastName = "Swift",
                TotalNumberOfRides = 3,
                TotalDistanceInKilometers = 120.4m,
            },
        };

        var service = Substitute.For<IStravaClubStatsService>();
        service.GetClubActivitiesSummariesAsync().Returns(expectedSummaries);

        var sut = new GetClubActivitiesSummariesHandler(service);

        var result = await sut.Handle(new GetClubActivitiesSummariesQuery(), CancellationToken.None);

        result.Should().BeSameAs(expectedSummaries);
        await service.Received(1).GetClubActivitiesSummariesAsync();
    }
}

[TestFixture]
public class GetClubStatsForYearHandlerTests
{
    [Test]
    public async Task Handle_ShouldReturnYearlyStatsFromServiceAsync()
    {
        var expectedStats = new List<StravaClubStatsForYear>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Cyclist = "Jordan Rider",
                Rides = 28,
                Distance = 845.6m,
            },
        };

        var service = Substitute.For<IStravaClubStatsForYearService>();
        service.GetStravaClubStatsForYearAsync().Returns(expectedStats);

        var sut = new GetClubStatsForYearHandler(service);

        var result = await sut.Handle(new GetClubStatsForYearQuery(), CancellationToken.None);

        result.Should().BeSameAs(expectedStats);
        await service.Received(1).GetStravaClubStatsForYearAsync();
    }
}
