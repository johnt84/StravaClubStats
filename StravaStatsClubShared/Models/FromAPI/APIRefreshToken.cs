namespace StravaClubStatsShared.Models.FromAPI;

public class RefreshAPIToken
{
    public string token_type { get; set; } = string.Empty;
    public string access_token { get; set; } = string.Empty;
    public int expires_at { get; set; } 
    public int expires_in { get; set; }
    public string refresh_token { get; set; } = string.Empty;
}
