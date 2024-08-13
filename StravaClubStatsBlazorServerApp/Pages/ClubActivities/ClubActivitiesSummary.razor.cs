using StravaClubStatsEngine.Queries;
using StravaClubStatsShared.Models;

namespace StravaClubStatsBlazorServerApp.Pages.ClubActivities;

public partial class ClubActivitiesSummary
{
    private List<ActivitiesSummary> clubActivitiesSummaries = new List<ActivitiesSummary>();

    private bool isInvalidClubActivities = false;

    private string errorMessage { get; set; } = string.Empty;

    private string searchText = string.Empty;

    private bool filterColumn(string columnName) => 
                            columnName.Contains(searchText, StringComparison.OrdinalIgnoreCase);

    private string ConvertTo2DecimalPlaces(decimal decimalValue) => decimalValue.ToString("0.##");

    private Func<ActivitiesSummary, bool> quickFilter => x =>
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return true;

        if (filterColumn($"{x.AthleteFirstName} {x.AthleteLastName}"))
            return true;

        if (filterColumn(x.TotalNumberOfRides.ToString()))
            return true;

        if (filterColumn(x.TotalDistanceInKilometers.ToString()))
            return true;

        if (filterColumn(x.LongestRideInKilometers.ToString()))
            return true;

        if (filterColumn(x.TotalElapsedTimeInHours.ToString()))
            return true;

        if (filterColumn(x.TotalMovingTimeInHours.ToString()))
            return true;

        if (filterColumn(x.TotalElevationGainInKilometers.ToString()))
            return true;

        if (filterColumn(x.AverageDistancePerRideInKilometers.ToString()))
            return true;

        if (filterColumn(x.AverageElapsedTimeInHours.ToString()))
            return true;

        if (filterColumn(x.AverageMovingTimeInHours.ToString()))
            return true;

        if (filterColumn(x.AverageElevationGainInKilometers.ToString()))
            return true;

        return false;
    };

    protected override async Task OnInitializedAsync()
    {
        await GetAllClubActivitiesSummariesAsync();
    }

    private async Task GetAllClubActivitiesSummariesAsync()
    {
        try
        {
            clubActivitiesSummaries = await Mediator.Send(new GetClubActivitiesSummariesQuery());
        }
        catch (Exception ex)
        {
            isInvalidClubActivities = true;
            errorMessage = $"Could not retrieve the club activities - {ex.Message}";
        }
    }
}
