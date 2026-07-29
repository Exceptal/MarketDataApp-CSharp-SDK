using MarketData.Tests.TestSupport;

namespace MarketData.Tests.Utilities;

public sealed class UtilitiesApiTests
{
    [Fact]
    public async Task GetStatusAsync_ParsesParallelArraysAndMetadata()
    {
        var handler = new StubHttpMessageHandler(_ => MarketDataTestClient.JsonResponse("""
        {
          "s": "ok",
          "service": ["/v1/stocks/quotes/"],
          "status": ["online"],
          "online": [true],
          "uptimePct30d": [0.99],
          "uptimePct90d": [0.98],
          "updated": [1706745600]
        }
        """));
        handler.ResponseHeaders.Add("x-request-id", "request-1");
        handler.ResponseHeaders.Add("x-api-ratelimit-limit", "100");
        handler.ResponseHeaders.Add("x-api-ratelimit-remaining", "99");
        handler.ResponseHeaders.Add("x-api-ratelimit-reset", "1706749200");
        handler.ResponseHeaders.Add("x-api-ratelimit-consumed", "1");

        var client = MarketDataTestClient.Create(handler);
        var response = await client.Utilities.GetStatusAsync();

        Assert.Equal("/v1/stocks/quotes/", response.Values[0].Service);
        Assert.True(response.Values[0].Online);
        Assert.Equal("request-1", response.RequestId);
        Assert.Equal(99, response.RateLimit!.Remaining);
        Assert.Equal("/status/", handler.LastRequest!.RequestUri!.AbsolutePath);
    }
}
