namespace StravaClubStatsShared.Models;

public class StravaClubStatsForYear
{
    public Guid Id { get; set; }
    public string Cyclist { get; set; } = string.Empty;
    public int Rides { get; set; }
    public string Time { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public decimal ElevationGain { get; set; }
    public decimal DistanceTarget { get; set; }
    public decimal DistanceLeftToDo { get; set; }
    public decimal DistanceTargetForCurrentWeek { get; set; }
    public decimal AverageDistanceToDoPerWeek { get; set; }
    public decimal AverageDistanceDonePerWeek { get; set; }
    public decimal AverageDistanceLeftToDoPerWeek { get; set; }
}
