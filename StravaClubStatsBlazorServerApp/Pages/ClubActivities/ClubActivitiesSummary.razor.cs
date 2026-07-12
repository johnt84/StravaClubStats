using Microsoft.AspNetCore.Components;
using StravaClubStatsShared.Models;

namespace StravaClubStatsBlazorServerApp.Pages.ClubActivities;

public partial class ClubActivitiesSummary
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!; 
    
    private List<ActivitiesSummary> ClubActivitiesSummaries = new List<ActivitiesSummary>();

    private List<string> Cyclists = new List<string>();

    private bool IsInvalidClubActivities = false;

    private string ErrorMessage { get; set; } = string.Empty;

    private string SearchText = string.Empty;

    private bool IsSmall = false;

    private bool FilterColumn(string columnName) => 
                            columnName.Contains(SearchText, StringComparison.OrdinalIgnoreCase);

    private string ConvertTo2DecimalPlaces(decimal decimalValue) => decimalValue.ToString("0.##");

    private Func<ActivitiesSummary, bool> quickFilter => x =>
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        if (FilterColumn($"{x.AthleteFirstName} {x.AthleteLastName}"))
            return true;

        if (FilterColumn(x.TotalNumberOfRides.ToString()))
            return true;

        if (FilterColumn(x.TotalDistanceInKilometers.ToString()))
            return true;

        if (FilterColumn(x.LongestRideInKilometers.ToString()))
            return true;

        if (FilterColumn(x.TotalElapsedTimeInHours.ToString()))
            return true;

        if (FilterColumn(x.TotalMovingTimeInHours.ToString()))
            return true;

        if (FilterColumn(x.TotalElevationGainInKilometers.ToString()))
            return true;

        if (FilterColumn(x.AverageDistancePerRideInKilometers.ToString()))
            return true;

        if (FilterColumn(x.AverageElapsedTimeInHours.ToString()))
            return true;

        if (FilterColumn(x.AverageMovingTimeInHours.ToString()))
            return true;

        if (FilterColumn(x.AverageElevationGainInKilometers.ToString()))
            return true;

        return false;
    };

    protected override void OnInitialized() =>
        NavigationManager.NavigateTo("/");


    // Removed access to the Summary and Drilldown pages as require a subscription to access the Strava API

    //private async Task GetAllClubActivitiesSummariesAsync()
    //{
    //    try
    //    {
    //        ClubActivitiesSummaries = await Mediator.Send(new GetClubActivitiesSummariesQuery());

    //        Cyclists = ClubActivitiesSummaries
    //                    .GroupBy(clubActivitySummary => clubActivitySummary.AthleteFirstName)
    //                    .Select(cyclist => cyclist.Key)
    //                    .OrderBy(cyclist => cyclist)
    //                    .ToList();

    //        if (IsSmall &&
    //            string.IsNullOrEmpty(SearchText) &&
    //            Cyclists.Any())
    //        {
    //            SearchText = Cyclists.First();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        IsInvalidClubActivities = true;
    //        ErrorMessage = $"Could not retrieve the club activities - {ex.Message}";
    //    }
    //}
}
