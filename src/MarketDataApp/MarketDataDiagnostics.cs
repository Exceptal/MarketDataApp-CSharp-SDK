using System.Diagnostics;

namespace MarketDataApp;

/// <summary>Diagnostics emitted by the Market Data SDK.</summary>
public static class MarketDataDiagnostics
{
    /// <summary>Name used by the SDK's <see cref="ActivitySource"/>.</summary>
    public const string ActivitySourceName = "MarketDataApp.SDK";

    /// <summary>Activity source for HTTP attempts and retry delays.</summary>
    public static ActivitySource ActivitySource { get; } = new(ActivitySourceName);
}
