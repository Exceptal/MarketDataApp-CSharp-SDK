using MarketData;
using MarketData.Stocks;
using MarketData.Tests.TestSupport;

namespace MarketData.Tests.Stocks;

public sealed class StocksApiTests
{
    [Fact]
    public async Task GetQuoteAsync_SendsBearerTokenAndEndpointParameters()
    {
        var handler = new StubHttpMessageHandler(_ => MarketDataTestClient.JsonResponse("""
        {
          "s": "ok",
          "symbol": ["AAPL"],
          "last": [190.25],
          "updated": [1706745600]
        }
        """));
        var client = MarketDataTestClient.Create(handler, "secret-token");

        var response = await client.Stocks.GetQuoteAsync(
            new StockQuoteRequest("AAPL") { Candle = true },
            new MarketDataRequestOptions { Mode = Mode.Live });

        Assert.Equal("AAPL", response.Values[0].Symbol);
        Assert.Equal(190.25, response.Values[0].Last);
        Assert.Equal("Bearer secret-token", handler.LastRequest!.Headers.Authorization!.ToString());
        Assert.Equal("/v1/stocks/quotes/AAPL/", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("candle=true", handler.LastRequest.RequestUri.Query);
        Assert.Contains("mode=live", handler.LastRequest.RequestUri.Query);
    }
}
