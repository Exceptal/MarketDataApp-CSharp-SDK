using MarketData;
using MarketData.Funds;
using MarketData.Tests.TestSupport;

namespace MarketData.Tests.Funds;

public sealed class FundsApiTests
{
    [Fact]
    public async Task GetCandlesAsync_MapsRequestAndParsesCandles()
    {
        var handler = new StubHttpMessageHandler(_ => MarketDataTestClient.JsonResponse("""
        {
          "s": "ok",
          "t": [1706745600],
          "o": [100.0],
          "h": [101.5],
          "l": [99.5],
          "c": [101.0]
        }
        """));
        var client = MarketDataTestClient.Create(handler, "secret-token");

        var response = await client.Funds.GetCandlesAsync(
            new FundCandlesRequest(FundResolution.Daily, "VTI")
            {
                From = new DateOnly(2024, 2, 1),
                To = new DateOnly(2024, 2, 2),
                Countback = 5
            },
            new MarketDataRequestOptions
            {
                DateFormat = DateFormat.Timestamp,
                Mode = Mode.Delayed,
                Limit = 10,
                Offset = 2,
                Columns = ["t", "c"]
            });

        Assert.Equal(101.0, response.Values[0].Close);
        Assert.Equal("Bearer", handler.LastRequest!.Headers.Authorization!.Scheme);
        Assert.Equal("/v1/funds/candles/D/VTI/", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("from=2024-02-01", handler.LastRequest.RequestUri.Query);
        Assert.Contains("to=2024-02-02", handler.LastRequest.RequestUri.Query);
        Assert.Contains("countback=5", handler.LastRequest.RequestUri.Query);
        Assert.Contains("dateformat=timestamp", handler.LastRequest.RequestUri.Query);
        Assert.Contains("mode=delayed", handler.LastRequest.RequestUri.Query);
        Assert.Contains("limit=10", handler.LastRequest.RequestUri.Query);
        Assert.Contains("offset=2", handler.LastRequest.RequestUri.Query);
        Assert.Contains("columns=t%2Cc", handler.LastRequest.RequestUri.Query);
    }

    [Fact]
    public async Task GetCandlesAsync_MapsSingleDateAndResolution()
    {
        var handler = new StubHttpMessageHandler(_ => MarketDataTestClient.JsonResponse("""
        {
          "s": "ok",
          "t": [1706745600],
          "c": [101.0]
        }
        """));
        var client = MarketDataTestClient.Create(handler);

        var response = await client.Funds.GetCandlesAsync(
            new FundCandlesRequest(FundResolution.Months(3), "VXUS")
            {
                Date = new DateOnly(2024, 2, 1)
            });

        Assert.Equal(101.0, response.Values[0].Close);
        Assert.Equal("/v1/funds/candles/3M/VXUS/", handler.LastRequest!.RequestUri!.AbsolutePath);
        Assert.Contains("date=2024-02-01", handler.LastRequest.RequestUri.Query);
    }
}
