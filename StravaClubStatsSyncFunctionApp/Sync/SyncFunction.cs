using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StravaClubStatsEngine;

namespace StravaClubStatsSyncFunctionApp.Sync;

public class SyncFunction
{
    private readonly ILogger<SyncFunction> _logger;
    private readonly StravaClubStatsSyncService _stravaClubStatsSyncService;
    private readonly IOptions<StravaAthleteSyncOptions> _options;

    public SyncFunction(
        ILogger<SyncFunction> logger,
        StravaClubStatsSyncService stravaClubStatsSyncService,
        IOptions<StravaAthleteSyncOptions> options)
    {
        _logger = logger;
        _stravaClubStatsSyncService = stravaClubStatsSyncService;
        _options = options;
    }

    [Function("SyncFunction")]
    public async Task RunAsync([TimerTrigger("%StravaAthleteSyncSchedule%")] TimerInfo myTimer, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Athlete stats sync started at: {executionTime}", DateTime.UtcNow);

        var options = _options.Value;
        var updatedAthleteCount = await _stravaClubStatsSyncService
            .SyncAsync(options.AthleteProfileUrls, options.TargetYear, cancellationToken);

        _logger.LogInformation("Athlete stats sync completed. Updated {updatedAthleteCount} athlete records.", updatedAthleteCount);

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}