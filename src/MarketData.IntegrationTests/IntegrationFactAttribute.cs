namespace MarketData.IntegrationTests;

internal sealed class IntegrationFactAttribute : FactAttribute
{
    public IntegrationFactAttribute()
    {
        if (!IntegrationTestConfiguration.Enabled)
        {
            Skip = "Configure MarketDataIntegrationTests:Enabled=true to run live integration tests.";
        }
        else if (string.IsNullOrWhiteSpace(IntegrationTestConfiguration.ApiToken))
        {
            Skip = "Configure MarketData:ApiToken to run live integration tests.";
        }
    }
}
