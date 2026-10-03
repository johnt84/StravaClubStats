namespace StravaClubStatsShared.Models.FromAPI;

public class StravaAthleteStats
{
    public StravaRideTotals ytd_ride_totals { get; set; } = new();
}

public class StravaRideTotals
{
    public int count { get; set; }
    public decimal distance { get; set; }
    public int moving_time { get; set; }
    public decimal elevation_gain { get; set; }
}
