using MarketData.Stocks;

namespace MarketData.IntegrationTests;

public sealed class StocksIntegrationTests : IntegrationTestBase
{
    [IntegrationFact]
    public async Task QuoteCandlesAndCsv_ReturnExpectedShapes()
    {
        var quote = await Client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL"));
        var candles = await Client.Stocks.GetCandlesAsync(
            new StockCandlesRequest(StockResolution.Daily, "AAPL")
            {
                To = DateOnly.FromDateTime(DateTime.UtcNow),
                Countback = 5
            });
        var csv = await Client.Stocks.GetPriceCsvAsync(new StockPriceRequest("AAPL"));

        AssertSuccess(quote.StatusCode);
        Assert.Contains(quote.Values, value => value.Symbol == "AAPL");
        AssertSuccess(candles.StatusCode);
        Assert.NotEmpty(candles.Values);
        AssertSuccess(csv.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(csv.Csv));
    }
}
