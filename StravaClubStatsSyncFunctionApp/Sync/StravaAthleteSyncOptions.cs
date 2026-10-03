namespace StravaClubStatsSyncFunctionApp.Sync;

public class StravaAthleteSyncOptions
{
    public const string SectionName = "StravaAthleteSync";

    public int TargetYear { get; set; } = 2026;

    public string[] AthleteProfileUrls { get; set; } =
    [
        "https://www.strava.com/athletes/117699788",
        "https://www.strava.com/athletes/96202847",
        "https://www.strava.com/athletes/99912880",
        "https://www.strava.com/athletes/3085505",
        "https://www.strava.com/athletes/32760466"
    ];
}
