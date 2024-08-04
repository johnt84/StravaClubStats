using StravaClubStatsEngine.Service.CosmosDb.Interface;
using StravaClubStatsEngine.Service.Interface;
using StravaClubStatsShared.Models;
using System.Globalization;

namespace StravaClubStatsEngine.Service;

public class StravaClubStatsForYearService : IStravaClubStatsForYearService
{
    public readonly ICosmosDbConnection _connection;
    
    public StravaClubStatsForYearService(ICosmosDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<List<StravaClubStatsForYear>> GetStravaClubStatsForYearAsync()
    {
        var clubStatsForYear = await _connection.QueryAsync();

        var stravaClubStatsForYear = clubStatsForYear.Select(record => new StravaClubStatsForYear
        {
            Id = new Guid(record.id),
            Cyclist = record.cyclist,
            Rides = Convert.ToInt32(record.rides),
            Time = record.time,
            Distance = Convert.ToDecimal(TidyDistance(record.distance)),
            ElevationGain = Convert.ToDecimal(TidyDistance(record.elevationgain)),
            DistanceTarget = Convert.ToDecimal(TidyDistance(record.distancetarget)),
        })
        .ToList();

        stravaClubStatsForYear.ForEach(record => CalculateDistances(record));

        return stravaClubStatsForYear;
    }

    public async Task<StravaClubStatsForYear?> GetStravaClubStatsForCyclistAsync(Guid cyclistId)
    {
        var clubStatsForCyclist = await _connection.GetCyclistStatsForYearAsync(cyclistId);

        if (clubStatsForCyclist is null)
        {
            return null;
        }

        return new StravaClubStatsForYear
        {
            Id = new Guid(clubStatsForCyclist.id),
            Cyclist = clubStatsForCyclist.cyclist,
            Rides = Convert.ToInt32(clubStatsForCyclist.rides),
            Time = clubStatsForCyclist.time,
            Distance = Convert.ToDecimal(TidyDistance(clubStatsForCyclist.distance)),
            ElevationGain = Convert.ToDecimal(TidyDistance(clubStatsForCyclist.elevationgain)),
            DistanceTarget = Convert.ToDecimal(TidyDistance(clubStatsForCyclist.distancetarget)),
        };
    }

    public async Task<bool> UpdateStravaClubStatsForYearAsync(Ride ride)
    {
        var stravaClubStatsForCyclist = await GetStravaClubStatsForCyclistAsync(ride.Id);

        if (stravaClubStatsForCyclist is null) 
        { 
            return false; 
        }    

        //To Do - Update time using datetime parse exact
        //var timeSpan = DateTime.ParseExact(clubStatsForCyclist.Time)
        stravaClubStatsForCyclist.Rides++;
        stravaClubStatsForCyclist.Distance += ride.Distance;
        //clubStatsForCyclist.Time
        stravaClubStatsForCyclist.ElevationGain += ride.ElevationGain;    

        return await _connection.UpdateStravaClubStatsForYearAsync(stravaClubStatsForCyclist);
    }

    private void CalculateDistances(StravaClubStatsForYear record)
    {
        const int NumberOfWeeksInYear = 52;

        int currentWeekNumber = GetCurrentWeekNumber();

        int weeksLeftInYear = NumberOfWeeksInYear - currentWeekNumber;

        record.DistanceLeftToDo = record.DistanceTarget - record.Distance;
        record.AverageDistanceToDoPerWeek = record.DistanceTarget / NumberOfWeeksInYear;
        record.AverageDistanceDonePerWeek = record.Distance / currentWeekNumber;
        record.AverageDistanceLeftToDoPerWeek = record.DistanceLeftToDo / weeksLeftInYear;
    }

    private int GetCurrentWeekNumber()
    {
        var dateFormatInfo = DateTimeFormatInfo.CurrentInfo;
        var calendar = dateFormatInfo.Calendar;

        return calendar.GetWeekOfYear(DateTime.UtcNow, dateFormatInfo.CalendarWeekRule,
                                            dateFormatInfo.FirstDayOfWeek);
    }

    private string TidyDistance(string distance) =>
        distance.Replace(" km", string.Empty).Replace(" m", string.Empty).Replace(",", string.Empty);
}
