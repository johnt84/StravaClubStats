using StravaClubStatsEngine.Service.API.Interface;
using StravaClubStatsShared.Models;
using System.Net.Http.Headers;

namespace StravaClubStatsEngine.Service.API;

public class HttpAPIClient : IHttpAPIClient
{
    public HttpClient _Client { get; }
    private readonly StravaClubStatsEngineInput _stravaClubStatsEngineInput;

    public HttpAPIClient(HttpClient httpClient, StravaClubStatsEngineInput stravaClubStatsEngineInput)
    {
        _stravaClubStatsEngineInput = stravaClubStatsEngineInput;
        _Client = httpClient;

        _Client.BaseAddress = new Uri(_stravaClubStatsEngineInput.StravaClubAPIUrl, UriKind.Absolute);
        _Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<string> GetAsync(string url)
    {
        return await _Client.GetStringAsync(url);
    }

    public async Task<HttpResponseMessage> PostAsync(string url)
    {
        var response = await _Client.PostAsync(url, null);

        response.EnsureSuccessStatusCode();

        return response;
    }
}
