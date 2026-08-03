using MarketData.Funds;

namespace MarketData.IntegrationTests;

public sealed class FundsIntegrationTests : IntegrationTestBase
{
    [IntegrationFact]
    public async Task Candles_ReturnRecentFundPrices()
    {
        var response = await Client.Funds.GetCandlesAsync(
            new FundCandlesRequest(FundResolution.Daily, "VFINX")
            {
                To = DateOnly.FromDateTime(DateTime.UtcNow),
                Countback = 5
            });

        AssertSuccess(response.StatusCode);
        Assert.NotEmpty(response.Values);
        Assert.Contains(response.Values, candle => candle.Close is > 0);
    }
}
