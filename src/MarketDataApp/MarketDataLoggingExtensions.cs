using Microsoft.Extensions.Logging;

namespace MarketDataApp;

/// <summary>Convenience extensions for attaching application logging to the SDK.</summary>
public static class MarketDataLoggingExtensions
{
    /// <summary>Returns options configured to emit SDK diagnostics through <paramref name="logger"/>.</summary>
    public static MarketDataClientOptions WithLogger(
        this MarketDataClientOptions options,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        return options with { Logger = logger };
    }
}
