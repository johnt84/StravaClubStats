using StravaClubStatsEngine.Queries;
using StravaClubStatsShared.Models;

namespace StravaClubStatsBlazorServerApp.Pages.ClubActivities;

public partial class ClubActivitiesDrilldown
{
    private List<Activity> clubActivities = new List<Activity>();

    private bool isInvalidClubActivities = false;

    private string errorMessage { get; set; } = string.Empty;

    private string searchText = string.Empty;

    private bool filterColumn(string columnName) =>
                    columnName.Contains(searchText, StringComparison.OrdinalIgnoreCase);

    private string ConvertTo2DecimalPlaces(decimal decimalValue) => decimalValue.ToString("0.##");

    private Func<Activity, bool> quickFilter => x =>
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return true;

        if (filterColumn(x.ActivityName))
            return true;

        if (filterColumn($"{x.AthleteFirstName} {x.AthleteLastName}"))
            return true;

        if(filterColumn(x.SportType))
            return true;

        if (filterColumn(x.DistanceInKilometers.ToString()))
            return true;

        if (filterColumn(x.ElapsedTimeInHours.ToString()))
            return true;

        if (filterColumn(x.MovingTimeInHours.ToString()))
            return true;

        if (filterColumn(x.TotalElevationGainInKilometers.ToString()))
            return true;

        return false;
    };

    protected override async Task OnInitializedAsync()
    {
        try
        {
            clubActivities = await Mediator.Send(new GetClubActivitiesQuery());
        }
        catch (Exception ex)
        {
            isInvalidClubActivities = true;
            errorMessage = $"Could not retrieve the club activities - {ex.Message}";
        }
    }
}
