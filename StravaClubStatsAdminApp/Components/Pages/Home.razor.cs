using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using StravaClubStatsEngine.Service.CosmosDb.Interface;
using StravaClubStatsShared.Models.FromAzure;

namespace StravaClubStatsAdminApp.Components.Pages;

public partial class Home
{
    private const string KilometersSuffix = " km";
    private const string MetresSuffix = " m";

    [Inject]
    private ICosmosDbConnection CosmosDbConnection { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<ClubStatsForYearEditorModel> Records { get; set; } = [];

    private ClubStatsForYearEditorModel? SelectedRecord { get; set; }

    private string SearchText { get; set; } = string.Empty;

    private string? LoadErrorMessage { get; set; }

    private string? SaveErrorMessage { get; set; }

    private bool IsLoading { get; set; } = true;

    private bool IsSaving { get; set; }

    private string FormTitle => SelectedRecord is null
        ? "Edit club stats"
        : $"Editing {SelectedRecord.Cyclist}";

    private IEnumerable<ClubStatsForYearEditorModel> FilteredRecords =>
        string.IsNullOrWhiteSpace(SearchText)
            ? Records
            : Records.Where(MatchesSearch);

    protected override async Task OnInitializedAsync() =>
        await LoadRecordsAsync();

    private async Task ReloadAsync() =>
        await LoadRecordsAsync(SelectedRecord?.Id);

    private async Task LoadRecordsAsync(string? selectedRecordId = null)
    {
        try
        {
            IsLoading = true;
            LoadErrorMessage = null;

            var records = await CosmosDbConnection.QueryAsync();

            Records = records
                .Select(ClubStatsForYearEditorModel.FromDocument)
                .OrderBy(record => record.Cyclist, StringComparer.OrdinalIgnoreCase)
                .ToList();

            SelectedRecord = Records
                .SingleOrDefault(record => record.Id == selectedRecordId)?
                .Clone();
        }
        catch (Exception ex)
        {
            LoadErrorMessage = $"Could not retrieve the club stats for the year - {ex.Message}";
            SelectedRecord = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BeginEdit(ClubStatsForYearEditorModel record)
    {
        SaveErrorMessage = null;
        SelectedRecord = record.Clone();
    }

    private void CancelEdit()
    {
        if (SelectedRecord is null)
        {
            return;
        }

        SaveErrorMessage = null;
        SelectedRecord = Records
            .Single(record => record.Id == SelectedRecord.Id)
            .Clone();
    }

    private async Task SaveAsync()
    {
        if (SelectedRecord is null)
        {
            return;
        }

        try
        {
            IsSaving = true;
            SaveErrorMessage = null;

            await CosmosDbConnection.UpsertAsync(SelectedRecord.ToDocument());
            await LoadRecordsAsync(SelectedRecord.Id);

            Snackbar.Add($"Saved changes for {SelectedRecord.Cyclist}.", Severity.Success);
        }
        catch (Exception ex)
        {
            SaveErrorMessage = $"Could not save the club stats record - {ex.Message}";
            Snackbar.Add(SaveErrorMessage, Severity.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool MatchesSearch(ClubStatsForYearEditorModel record)
    {
        var search = SearchText.Trim();

        return record.Cyclist.Contains(search, StringComparison.OrdinalIgnoreCase)
            || record.Time.Contains(search, StringComparison.OrdinalIgnoreCase)
            || record.Rides.ToString(CultureInfo.InvariantCulture).Contains(search, StringComparison.OrdinalIgnoreCase)
            || record.Distance.ToString("0.##", CultureInfo.InvariantCulture).Contains(search, StringComparison.OrdinalIgnoreCase)
            || record.ElevationGain.ToString("0.##", CultureInfo.InvariantCulture).Contains(search, StringComparison.OrdinalIgnoreCase)
            || record.DistanceTarget.ToString("0.##", CultureInfo.InvariantCulture).Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private static int ParseInt(string value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue)
            ? parsedValue
            : 0;

    private static decimal ParseDecimal(string value)
    {
        var normalizedValue = value
            .Replace(KilometersSuffix, string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(MetresSuffix, string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Trim();

        return decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue)
            ? parsedValue
            : 0;
    }

    private static string FormatDistance(decimal value) =>
        string.Create(CultureInfo.InvariantCulture, $"{value:0.##}{KilometersSuffix}");

    private static string FormatElevationGain(decimal value) =>
        string.Create(CultureInfo.InvariantCulture, $"{value:0.##}{MetresSuffix}");

    private sealed class ClubStatsForYearEditorModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Cyclist { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Rides { get; set; }

        [Required]
        public string Time { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal Distance { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal ElevationGain { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal DistanceTarget { get; set; }

        public static ClubStatsForYearEditorModel FromDocument(ClubStatsForYear document) =>
            new()
            {
                Id = document.id,
                Cyclist = document.cyclist,
                Rides = ParseInt(document.rides),
                Time = document.time,
                Distance = ParseDecimal(document.distance),
                ElevationGain = ParseDecimal(document.elevationgain),
                DistanceTarget = ParseDecimal(document.distancetarget),
            };

        public ClubStatsForYearEditorModel Clone() =>
            new()
            {
                Id = Id,
                Cyclist = Cyclist,
                Rides = Rides,
                Time = Time,
                Distance = Distance,
                ElevationGain = ElevationGain,
                DistanceTarget = DistanceTarget,
            };

        public ClubStatsForYear ToDocument() =>
            new()
            {
                id = Id,
                cyclist = Cyclist.Trim(),
                rides = Rides.ToString(CultureInfo.InvariantCulture),
                time = Time.Trim(),
                distance = FormatDistance(Distance),
                elevationgain = FormatElevationGain(ElevationGain),
                distancetarget = FormatDistance(DistanceTarget),
            };
    }
}
