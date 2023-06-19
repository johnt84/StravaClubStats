﻿using StravaClubStatsEngine.Queries;
using StravaClubStatsShared.Models;

namespace StravaClubStatsBlazorServerApp.Pages.ClubActivities
{
    public partial class ClubActivitiesSummary
    {
        private List<ActivitiesSummary> clubActivitiesSummaries = null;

        private bool isInvalidClubActivities = false;

        private string errorMessage { get; set; }

        private string searchText;

        private bool filterColumn(string columnName) => 
                                columnName.Contains(searchText, StringComparison.OrdinalIgnoreCase);

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

            return false;
        };

        protected override async Task OnInitializedAsync()
        {
            await GetAllClubActivitiesSummaries();
        }

        private async Task GetAllClubActivitiesSummaries()
        {
            try
            {
                clubActivitiesSummaries = await Mediator.Send(new GetClubActivitiesSummariesQuery());
            }
            catch (Exception ex)
            {
                isInvalidClubActivities = true;
                errorMessage = $"Could not retrieve the club activities";
            }
        }
    }
}
