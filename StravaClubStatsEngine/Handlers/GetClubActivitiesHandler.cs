using MediatR;
using StravaClubStatsEngine.Queries;
using StravaClubStatsEngine.Service.Interface;
using StravaClubStatsShared.Models;

namespace StravaClubStatsEngine.Handlers;

public class GetClubActivitiesHandler : IRequestHandler<GetClubActivitiesQuery, List<Activity>>
{
    private readonly IStravaClubStatsService _stravaClubStatsService;

    public GetClubActivitiesHandler(IStravaClubStatsService stravaClubStatsService)
    {
        _stravaClubStatsService = stravaClubStatsService;
    }

    public async Task<List<Activity>> Handle(GetClubActivitiesQuery request, CancellationToken token)
    {
        return await _stravaClubStatsService.GetClubActivitiesAsync();
    }
}
