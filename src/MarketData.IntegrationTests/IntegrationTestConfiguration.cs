using DotNetEnv;
using DotNetEnv.Configuration;
using Microsoft.Extensions.Configuration;

namespace MarketData.IntegrationTests;

internal static class IntegrationTestConfiguration
{
    public static IConfiguration Instance { get; } = BuildConfiguration();

    private static IConfiguration BuildConfiguration()
    {
        var environmentFile = Path.Combine(AppContext.BaseDirectory, ".env");
        var builder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MARKETDATA_MAX_RETRIES"] = "1"
            });

        if (File.Exists(environmentFile))
        {
            builder.AddDotNetEnv(environmentFile, new LoadOptions(clobberExistingVars: false));
        }

        return builder.AddEnvironmentVariables().Build();
    }

    public static string? ApiToken => Instance["MARKETDATA_TOKEN"];

    public static bool Enabled =>
        bool.TryParse(Instance["MARKETDATA_RUN_INTEGRATION_TESTS"], out var enabled)
        && enabled;
}
