using MarketData.Options;

namespace MarketData.IntegrationTests;

public sealed class OptionsIntegrationTests : IntegrationTestBase
{
    [IntegrationFact]
    public async Task ExpirationsChainAndStrikes_ReturnExpectedShapes()
    {
        var expirations = await Client.Options.GetExpirationsAsync(
            new OptionsExpirationsRequest("AAPL"));
        var chain = await Client.Options.GetChainAsync(
            new OptionsChainRequest("AAPL")
            {
                Side = OptionSide.Call,
                StrikeLimit = 2
            });
        var strikes = await Client.Options.GetStrikesAsync(
            new OptionsStrikesRequest("AAPL"));

        AssertSuccess(expirations.StatusCode);
        Assert.NotEmpty(expirations.Values);
        AssertSuccess(chain.StatusCode);
        Assert.NotEmpty(chain.Values);
        AssertSuccess(strikes.StatusCode);
        Assert.NotEmpty(strikes.Values.ByExpiration);
    }
}
