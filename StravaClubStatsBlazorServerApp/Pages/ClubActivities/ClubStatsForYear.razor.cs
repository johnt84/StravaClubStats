using StravaClubStatsEngine.Queries;
using StravaClubStatsShared.Models;

namespace StravaClubStatsBlazorServerApp.Pages.ClubActivities;

public partial class ClubStatsForYear
{
    private List<StravaClubStatsForYear> clubStatsForYear = new List<StravaClubStatsForYear>();

    private bool isInvalidClubStatsForYear = false;

    private string errorMessage { get; set; } = string.Empty;

    private string searchText = string.Empty;

    private bool filterColumn(string columnName) =>
                            columnName.Contains(searchText, StringComparison.OrdinalIgnoreCase);

    private string ConvertTo2DecimalPlaces(decimal decimalValue) => decimalValue.ToString("0.##");

    private Func<StravaClubStatsForYear, bool> quickFilter => x =>
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return true;

        if (filterColumn(x.Cyclist))
            return true;

        if (filterColumn(x.Rides.ToString()))
            return true;

        if (filterColumn(x.Time))
            return true;

        if (filterColumn(x.Distance.ToString()))
            return true;

        if (filterColumn(x.ElevationGain.ToString()))
            return true;

        if (filterColumn(x.DistanceTarget.ToString()))
            return true;

        if (filterColumn(x.DistanceLeftToDo.ToString()))
            return true;

        if (filterColumn(x.AverageDistanceToDoPerWeek.ToString()))
            return true;

        if (filterColumn(x.AverageDistanceDonePerWeek.ToString()))
            return true;

        if (filterColumn(x.AverageDistanceLeftToDoPerWeek.ToString()))
            return true;

        return false;
    };

    private string GetDistanceDoneColour(decimal distance, decimal distanceTargetForCurrentWeek) =>
        distance < distanceTargetForCurrentWeek ? "color:#FF0000" : "color:#000000";

    private string GetAverageDoneColour(decimal avaregeDonePerWeek, decimal averageToDoPerWeek) =>
        avaregeDonePerWeek < averageToDoPerWeek ? "color:#FF0000" : "color:#000000";

    private string GetAverageLeftToDoColour(decimal avaregeLeftToDoPerWeek, decimal averageToDoPerWeek) =>
        avaregeLeftToDoPerWeek > averageToDoPerWeek ? "color:#FF0000" : "color:#000000";

    protected override async Task OnInitializedAsync()
    {
        await GetStravaClubStatsForYearAsync();
    }

    private async Task GetStravaClubStatsForYearAsync()
    {
        try
        {
            clubStatsForYear = await Mediator.Send(new GetClubStatsForYearQuery());
        }
        catch (Exception ex)
        {
            isInvalidClubStatsForYear = true;
            errorMessage = $"Could not retrieve the club stats for the year - {ex.Message}";
        }
    }
}
