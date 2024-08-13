using MediatR;
using StravaClubStatsEngine.Queries;
using StravaClubStatsEngine.Service.Interface;
using StravaClubStatsShared.Models;

namespace StravaClubStatsEngine.Handlers;

public class GetClubStatsForYearHandler : IRequestHandler<GetClubStatsForYearQuery, List<StravaClubStatsForYear>>
{
    private readonly IStravaClubStatsForYearService _stravaClubStatsForYearService;

    public GetClubStatsForYearHandler(IStravaClubStatsForYearService stravaClubStatsForYearService)
    {
        _stravaClubStatsForYearService = stravaClubStatsForYearService;
    }

    public async Task<List<StravaClubStatsForYear>> Handle(GetClubStatsForYearQuery request, CancellationToken token)
    {
        return await _stravaClubStatsForYearService.GetStravaClubStatsForYearAsync();
    }
}
