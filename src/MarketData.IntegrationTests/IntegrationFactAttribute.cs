namespace MarketData.IntegrationTests;

internal sealed class IntegrationFactAttribute : FactAttribute
{
    public IntegrationFactAttribute()
    {
        if (!IntegrationTestConfiguration.Enabled)
        {
            Skip = "Configure MARKETDATA_RUN_INTEGRATION_TESTS=true to run live integration tests.";
        }
        else if (string.IsNullOrWhiteSpace(IntegrationTestConfiguration.ApiToken))
        {
            Skip = "Configure MARKETDATA_TOKEN to run live integration tests.";
        }
    }
}
