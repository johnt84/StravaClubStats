namespace StravaClubStatsShared.Models;

public class Ride
{
    public Guid Id { get; set; }
    public string Cyclist { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string Time { get; set; } = string.Empty;
    public decimal ElevationGain { get; set; }
}
