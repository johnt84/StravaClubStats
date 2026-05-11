using StravaClubStatsEngine.Queries;
using StravaClubStatsShared.Models;

namespace StravaClubStatsBlazorServerApp.Components;

public partial class ClubStatsForYear
{
    private List<StravaClubStatsForYear> ClubStatsForYears = new List<StravaClubStatsForYear>();

    private List<string> Cyclists = new List<string>();

    private bool IsInvalidClubStatsForYear = false;

    private string ErrorMessage { get; set; } = string.Empty;

    private string SearchText = string.Empty;

    private bool FilterColumn(string columnName) =>
                            columnName.Contains(SearchText, StringComparison.OrdinalIgnoreCase);

    private string ConvertTo2DecimalPlaces(decimal decimalValue) => decimalValue.ToString("0.##");

    private bool IsSmall = false;

    private const string Red = "color:#FF0000";

    private const string Green = "color:#008000";

    private const string Black = "color:#000000";

    private Func<StravaClubStatsForYear, bool> quickFilter => x =>
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        if (FilterColumn(x.Cyclist))
            return true;

        if (FilterColumn(x.Rides.ToString()))
            return true;

        if (FilterColumn(x.Time))
            return true;

        if (FilterColumn(x.Distance.ToString()))
            return true;

        if (FilterColumn(x.ElevationGain.ToString()))
            return true;

        if (FilterColumn(x.DistanceTarget.ToString()))
            return true;

        if (FilterColumn(x.DistanceLeftToDo.ToString()))
            return true;

        if (FilterColumn(x.AverageDistanceToDoPerWeek.ToString()))
            return true;

        if (FilterColumn(x.AverageDistanceDonePerWeek.ToString()))
            return true;

        if (FilterColumn(x.AverageDistanceLeftToDoPerWeek.ToString()))
            return true;

        return false;
    };

    private string GetDistanceDoneColour(decimal distance, decimal distanceTargetForCurrentWeek) =>
        distance < distanceTargetForCurrentWeek ? Red : Black;

    private string GetDoneColour(decimal distance, decimal distanceTargetForYear, decimal distanceTargetForCurrentWeek)
    {
        if (distance >= distanceTargetForYear)
        {
            return Green;
        }

        if (distance < distanceTargetForCurrentWeek)
        {
            return Red;
        }

        return Black;
    }

    private string GetAverageDoneColour(decimal avaregeDonePerWeek, decimal averageToDoPerWeek) =>
        avaregeDonePerWeek < averageToDoPerWeek ? Red : Black;

    private string GetAverageLeftToDoColour(decimal avaregeLeftToDoPerWeek, decimal averageToDoPerWeek) =>
        avaregeLeftToDoPerWeek > averageToDoPerWeek ? Red : Black;

    protected override async Task OnInitializedAsync()
    {
        await GetStravaClubStatsForYearAsync();
    }

    private async Task GetStravaClubStatsForYearAsync()
    {
        try
        {
            ClubStatsForYears = await Mediator.Send(new GetClubStatsForYearQuery());

            Cyclists = ClubStatsForYears
                        .GroupBy(clubStatsForYear => clubStatsForYear.Cyclist)
                        .Select(cyclist => cyclist.Key)
                        .ToList();

            if (IsSmall && 
                string.IsNullOrEmpty(SearchText) && 
                Cyclists.Any())
            {
                SearchText = Cyclists.First();
            }
        }
        catch (Exception ex)
        {
            IsInvalidClubStatsForYear = true;
            ErrorMessage = $"Could not retrieve the club stats for the year - {ex.Message}";
        }
    }
}
