namespace StravaClubStatsShared.Models;

public class StravaClubStatsEngineInput
{
    public string StravaClubAPIUrl { get; set; } = string.Empty;
    public int ClientID { get; set; }
    public string ClientSecret { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ClubID { get; set; }
    public int NumberOfPages { get; set; }
    public string CosmosDbEndointUrl { get; set; } = string.Empty;
    public string CosmosDbPrimaryKey { get; set; } = string.Empty;
    public string CosmosDatabase { get; set; } = string.Empty;
    public string CosmosPartitionKey { get; set; } = string.Empty;
    public bool AllowTargetUpdates { get; set; } = false;
}
