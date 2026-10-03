namespace StravaClubStatsShared.Models;

public class StravaAthleteYearToDateCyclingStats
{
    public int AthleteId { get; set; }
    public string AthleteName { get; set; } = string.Empty;
    public int Activities { get; set; }
    public decimal DistanceInKilometers { get; set; }
    public decimal ElevationGainInMetres { get; set; }
    public string Time { get; set; } = string.Empty;
    public string AthleteProfileUrl { get; set; } = string.Empty;
}
