using Microsoft.Extensions.Configuration;

namespace MarketData.IntegrationTests;

internal static class IntegrationTestConfiguration
{
    public static IConfiguration Instance { get; } = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MarketData:MaxRetries"] = "1"
        })
        .AddUserSecrets(typeof(IntegrationTestConfiguration).Assembly, optional: true)
        .AddEnvironmentVariables()
        .Build();

    public static string? ApiToken => Instance["MarketData:ApiToken"];

    public static bool Enabled =>
        bool.TryParse(Instance["MarketDataIntegrationTests:Enabled"], out var enabled)
        && enabled;
}
