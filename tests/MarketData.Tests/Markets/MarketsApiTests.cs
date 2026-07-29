using MarketData;
using MarketData.Markets;
using MarketData.Tests.TestSupport;

namespace MarketData.Tests.Markets;

public sealed class MarketsApiTests
{
    [Fact]
    public async Task GetStatusAsync_IncludesVersionAndEndpointParameters()
    {
        var handler = new StubHttpMessageHandler(_ => MarketDataTestClient.JsonResponse("""
        {
          "s": "ok",
          "date": [1706745600],
          "status": ["open"]
        }
        """));
        var client = MarketDataTestClient.Create(handler);

        var response = await client.Markets.GetStatusAsync(
            new MarketStatusRequest
            {
                Country = "US",
                From = new DateOnly(2024, 2, 1),
                To = new DateOnly(2024, 2, 2)
            },
            new MarketDataRequestOptions
            {
                DateFormat = DateFormat.Timestamp,
                Limit = 10
            });

        Assert.True(response.Values[0].IsOpen);
        var uri = handler.LastRequest!.RequestUri!;
        Assert.Equal("/v1/markets/status/", uri.AbsolutePath);
        Assert.Contains("country=US", uri.Query);
        Assert.Contains("from=2024-02-01", uri.Query);
        Assert.Contains("dateformat=timestamp", uri.Query);
        Assert.Contains("limit=10", uri.Query);
    }
}
