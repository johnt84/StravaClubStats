using System.Globalization;
using FluentAssertions;
using NSubstitute;
using StravaClubStatsEngine.Service;
using StravaClubStatsEngine.Service.CosmosDb.Interface;
using StravaClubStatsShared.Models.FromAzure;

namespace StravaClubStatsEngineUnitTests.Service;

[TestFixture]
public class StravaClubStatsForYearServiceTests
{
    [Test]
    public async Task GetStravaClubStatsForYearAsync_ShouldMapRecordsAndCalculateWeeklyMetricsAsync()
    {
        var sourceRecord = new ClubStatsForYear
        {
            id = "5b743f06-db0b-49f4-b3df-feb8fd1007bc",
            cyclist = "Alex Rider",
            rides = "42",
            time = "120h 30m",
            distance = "1,234 km",
            elevationgain = "12,345 m",
            distancetarget = "2,000 km",
        };

        var connection = Substitute.For<ICosmosDbConnection>();
        connection.QueryAsync().Returns(new List<ClubStatsForYear> { sourceRecord });

        var sut = new StravaClubStatsForYearService(connection);

        var result = await sut.GetStravaClubStatsForYearAsync();

        var currentWeekNumber = GetCurrentWeekNumber();
        var weeksLeft = Math.Max(52 - currentWeekNumber, 1);

        result.Should().ContainSingle();
        result[0].Should().BeEquivalentTo(new
        {
            Id = Guid.Parse(sourceRecord.id),
            Cyclist = "Alex Rider",
            Rides = 42,
            Time = "120h 30m",
            Distance = 1234m,
            ElevationGain = 12345m,
            DistanceTarget = 2000m,
            DistanceLeftToDo = 766m,
            AverageDistanceToDoPerWeek = 2000m / 52m,
            AverageDistanceDonePerWeek = 1234m / currentWeekNumber,
            AverageDistanceLeftToDoPerWeek = 766m / weeksLeft,
            DistanceTargetForCurrentWeek = (2000m / 52m) * currentWeekNumber,
        });

        await connection.Received(1).QueryAsync();
    }

    private static int GetCurrentWeekNumber()
    {
        var dateFormatInfo = DateTimeFormatInfo.CurrentInfo!;
        var calendar = dateFormatInfo.Calendar;

        return calendar.GetWeekOfYear(DateTime.UtcNow, dateFormatInfo.CalendarWeekRule, dateFormatInfo.FirstDayOfWeek);
    }
}
