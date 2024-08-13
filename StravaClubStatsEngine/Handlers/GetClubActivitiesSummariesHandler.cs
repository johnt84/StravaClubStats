using MediatR;
using StravaClubStatsEngine.Queries;
using StravaClubStatsEngine.Service.Interface;
using StravaClubStatsShared.Models;

namespace StravaClubStatsEngine.Handlers;

public class GetClubActivitiesSummariesHandler : IRequestHandler<GetClubActivitiesSummariesQuery, List<ActivitiesSummary>>
{
    private readonly IStravaClubStatsService _stravaClubStatsService;

    public GetClubActivitiesSummariesHandler(IStravaClubStatsService stravaClubStatsService)
    {
        _stravaClubStatsService = stravaClubStatsService;
    }

    public async Task<List<ActivitiesSummary>> Handle(GetClubActivitiesSummariesQuery request, CancellationToken token)
    {
        return await _stravaClubStatsService.GetClubActivitiesSummariesAsync();
    }
}
