namespace MarketData.IntegrationTests;

internal sealed class IntegrationFactAttribute : FactAttribute
{
    private const string EnabledVariable = "MARKETDATA_RUN_INTEGRATION_TESTS";
    private const string TokenVariable = "MARKETDATA_TOKEN";

    public IntegrationFactAttribute()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(EnabledVariable),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Skip = $"Set {EnabledVariable}=true to run live integration tests.";
        }
        else if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(TokenVariable)))
        {
            Skip = $"Set {TokenVariable} to run live integration tests.";
        }
    }
}
