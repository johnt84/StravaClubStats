using FluentAssertions;
using NSubstitute;
using StravaClubStatsEngine;
using StravaClubStatsEngine.Service.CosmosDb.Interface;
using StravaClubStatsEngine.Service.Interface;
using StravaClubStatsShared.Models;
using StravaClubStatsShared.Models.FromAzure;

namespace StravaClubStatsEngineUnitTests;

[TestFixture]
public class StravaClubStatsSyncServiceTests
{
    [Test]
    public async Task SyncAsync_ShouldUpdateMatchingCosmosRecordWithScrapedCyclingStatsAsync()
    {
        var scrapedAthlete = new StravaAthleteYearToDateCyclingStats
        {
            AthleteName = "John   Tomlinson",
            Activities = 12,
            DistanceInKilometers = 345.67m,
            ElevationGainInMetres = 4567m,
            Time = "18h 22m",
            AthleteProfileUrl = "https://www.strava.com/athletes/32760466",
        };

        var existingRecord = new ClubStatsForYear
        {
            id = "record-1",
            cyclist = " John Tomlinson ",
            rides = "0",
            time = "0h",
            distance = "0 km",
            elevationgain = "0 m",
            distancetarget = "500 km",
        };

        var cosmosDbConnection = Substitute.For<ICosmosDbConnection>();
        cosmosDbConnection.QueryAsync().Returns([existingRecord]);

        var athleteStatsService = Substitute.For<IStravaAthleteStatsService>();
        athleteStatsService
            .GetCyclingYearToDateStatsAsync(Arg.Any<IEnumerable<string>>(), 2026, Arg.Any<CancellationToken>())
            .Returns([scrapedAthlete]);

        var sut = new StravaClubStatsSyncService(cosmosDbConnection, athleteStatsService);

        var updatedCount = await sut.SyncAsync([scrapedAthlete.AthleteProfileUrl], 2026);

        updatedCount.Should().Be(1);
        existingRecord.rides.Should().Be("12");
        existingRecord.time.Should().Be("18h 22m");
        existingRecord.distance.Should().Be("345.67 km");
        existingRecord.elevationgain.Should().Be("4567 m");
        existingRecord.distancetarget.Should().Be("500 km");
        await cosmosDbConnection.Received(1).UpsertAsync(existingRecord);
    }

    [Test]
    public async Task SyncAsync_ShouldSkipScrapedAthletesThatDoNotMatchCosmosRecordsAsync()
    {
        var cosmosDbConnection = Substitute.For<ICosmosDbConnection>();
        cosmosDbConnection.QueryAsync().Returns([
            new ClubStatsForYear { id = "record-1", cyclist = "Alice Rider" }
        ]);

        var athleteStatsService = Substitute.For<IStravaAthleteStatsService>();
        athleteStatsService
            .GetCyclingYearToDateStatsAsync(Arg.Any<IEnumerable<string>>(), 2026, Arg.Any<CancellationToken>())
            .Returns([
                new StravaAthleteYearToDateCyclingStats
                {
                    AthleteName = "Bob Climber",
                    Activities = 5,
                    DistanceInKilometers = 100m,
                    ElevationGainInMetres = 1200m,
                    Time = "5h",
                    AthleteProfileUrl = "https://www.strava.com/athletes/1",
                }
            ]);

        var sut = new StravaClubStatsSyncService(cosmosDbConnection, athleteStatsService);

        var updatedCount = await sut.SyncAsync(["https://www.strava.com/athletes/1"], 2026);

        updatedCount.Should().Be(0);
        await cosmosDbConnection.DidNotReceive().UpsertAsync(Arg.Any<ClubStatsForYear>());
    }
}
