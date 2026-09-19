using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using StravaClubStatsEngine.Service.API;
using StravaClubStatsShared.Models;

namespace StravaClubStatsEngineUnitTests.Service;

[TestFixture]
public class HttpAPIClientTests
{
    [Test]
    public async Task GetAsync_ShouldUseConfiguredBaseAddressAndJsonAcceptHeaderAsync()
    {
        var handler = new StubHttpMessageHandler
        {
            ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("payload"),
            },
        };

        using var httpClient = new HttpClient(handler);
        var input = CreateInput();
        var sut = new HttpAPIClient(httpClient, input);

        var result = await sut.GetAsync("clubs/99/activities");

        result.Should().Be("payload");
        handler.Requests.Should().ContainSingle();
        handler.Requests[0].Method.Should().Be(HttpMethod.Get);
        handler.Requests[0].RequestUri.Should().Be(new Uri("https://www.strava.com/api/v3/clubs/99/activities"));
        sut._Client.BaseAddress.Should().Be(new Uri(input.StravaClubAPIUrl));
        sut._Client.DefaultRequestHeaders.Accept.Should().ContainSingle(x =>
            x.MediaType == "application/json");
    }

    [Test]
    public async Task PostAsync_ShouldThrowWhenResponseIsNotSuccessfulAsync()
    {
        var handler = new StubHttpMessageHandler
        {
            ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.BadRequest),
        };

        using var httpClient = new HttpClient(handler);
        var sut = new HttpAPIClient(httpClient, CreateInput());

        var action = async () => await sut.PostAsync("oauth/token");

        await action.Should().ThrowAsync<HttpRequestException>();
        handler.Requests.Should().ContainSingle();
        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri.Should().Be(new Uri("https://www.strava.com/api/v3/oauth/token"));
    }

    private static StravaClubStatsEngineInput CreateInput() => new()
    {
        StravaClubAPIUrl = "https://www.strava.com/api/v3/",
    };

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        public required Func<HttpRequestMessage, HttpResponseMessage> ResponseFactory { get; init; }

        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(ResponseFactory(request));
        }
    }
}
