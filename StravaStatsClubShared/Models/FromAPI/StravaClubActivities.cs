namespace StravaClubStatsShared.Models.FromAPI;

public class StravaClubActivities
{
    public int resource_state { get; set; }
    public Athlete athlete { get; set; } = null!;
    public string name { get; set; } = string.Empty;
    public float distance { get; set; }
    public int moving_time { get; set; }
    public int elapsed_time { get; set; }
    public float total_elevation_gain { get; set; }
    public string type { get; set; } = string.Empty;
    public string sport_type { get; set; } = string.Empty;
    public int? workout_type { get; set; }
}

public class Athlete
{
    public int resource_state { get; set; }
    public string firstname { get; set; } = string.Empty;
    public string lastname { get; set; } = string.Empty;
}
