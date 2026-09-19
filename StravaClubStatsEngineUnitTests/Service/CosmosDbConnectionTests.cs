using System.Collections;
using System.Net;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using StravaClubStatsEngine.Service.CosmosDb;
using StravaClubStatsShared.Models;
using StravaClubStatsShared.Models.FromAzure;

namespace StravaClubStatsEngineUnitTests.Service;

[TestFixture]
public class CosmosDbConnectionTests
{
    [Test]
    public async Task QueryAsync_ShouldReturnItemsFromAllFeedPagesAsync()
    {
        var expectedItems = new[]
        {
            new ClubStatsForYear { id = "1", cyclist = "Alex Rider" },
            new ClubStatsForYear { id = "2", cyclist = "Jamie Climber" },
        };

        var feedIterator = new TestFeedIterator(
            new TestFeedResponse([expectedItems[0]]),
            new TestFeedResponse([expectedItems[1]]));

        var container = Substitute.For<Container>();
        container
            .GetItemQueryIterator<ClubStatsForYear>(Arg.Any<QueryDefinition>(), Arg.Any<string?>(), Arg.Any<QueryRequestOptions?>())
            .Returns(feedIterator);

        var sut = new CosmosDbConnection(CreateInput(), () => Task.FromResult(container));

        var result = await sut.QueryAsync();

        result.Should().BeEquivalentTo(expectedItems);
        container.Received(1)
            .GetItemQueryIterator<ClubStatsForYear>(Arg.Any<QueryDefinition>(), Arg.Any<string?>(), Arg.Any<QueryRequestOptions?>());
    }

    [Test]
    public async Task UpsertAsync_ShouldPersistItemUsingItsIdAsPartitionKeyAsync()
    {
        var item = new ClubStatsForYear
        {
            id = "record-42",
            cyclist = "Alex Rider",
            rides = "12",
        };

        PartitionKey? capturedPartitionKey = null;
        ClubStatsForYear? capturedItem = null;

        var container = Substitute.For<Container>();
        container
            .UpsertItemAsync(
                Arg.Any<ClubStatsForYear>(),
                Arg.Any<PartitionKey?>(),
                Arg.Any<ItemRequestOptions?>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedItem = callInfo.Arg<ClubStatsForYear>();
                capturedPartitionKey = callInfo.ArgAt<PartitionKey?>(1);
                return Task.FromResult(Substitute.For<ItemResponse<ClubStatsForYear>>());
            });

        var sut = new CosmosDbConnection(CreateInput(), () => Task.FromResult(container));

        await sut.UpsertAsync(item);

        capturedItem.Should().BeSameAs(item);
        capturedPartitionKey.Should().Be(new PartitionKey(item.id));
    }

    private static StravaClubStatsEngineInput CreateInput() => new();

    private sealed class TestFeedIterator(params FeedResponse<ClubStatsForYear>[] pages) : FeedIterator<ClubStatsForYear>
    {
        private readonly Queue<FeedResponse<ClubStatsForYear>> _pages = new(pages);

        public override bool HasMoreResults => _pages.Count > 0;

        public override Task<FeedResponse<ClubStatsForYear>> ReadNextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_pages.Dequeue());
    }

    private sealed class TestFeedResponse(IEnumerable<ClubStatsForYear> items) : FeedResponse<ClubStatsForYear>
    {
        private readonly IReadOnlyList<ClubStatsForYear> _items = items.ToList();

        public override IEnumerable<ClubStatsForYear> Resource => _items;

        public override HttpStatusCode StatusCode => HttpStatusCode.OK;

        public override CosmosDiagnostics Diagnostics => Substitute.For<CosmosDiagnostics>();

        public override string ContinuationToken => string.Empty;

        public override int Count => _items.Count;

        public override Headers Headers => new Headers();

        public override string IndexMetrics => string.Empty;

        public override IEnumerator<ClubStatsForYear> GetEnumerator() => _items.GetEnumerator();
    }
}
