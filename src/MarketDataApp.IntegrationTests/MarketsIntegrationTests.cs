using MarketDataApp.Markets;

namespace MarketDataApp.IntegrationTests;

public sealed class MarketsIntegrationTests : IntegrationTestBase
{
    [IntegrationFact]
    public async Task Status_ReturnsRecentTradingDays()
    {
        var response = await Client.Markets.GetStatusAsync(
            new MarketStatusRequest
            {
                To = DateOnly.FromDateTime(DateTime.UtcNow),
                Countback = 5,
                Country = "US"
            });

        AssertSuccess(response.StatusCode);
        Assert.NotEmpty(response.Values);
        Assert.All(response.Values, value => Assert.Contains(value.Status, new[] { "open", "closed" }));
    }
}
