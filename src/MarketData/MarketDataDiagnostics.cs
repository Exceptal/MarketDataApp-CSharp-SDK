using System.Diagnostics;

namespace MarketData;

/// <summary>Diagnostics emitted by the Market Data SDK.</summary>
public static class MarketDataDiagnostics
{
    /// <summary>Name used by the SDK's <see cref="ActivitySource"/>.</summary>
    public const string ActivitySourceName = "MarketData.SDK";

    /// <summary>Activity source for HTTP attempts and retry delays.</summary>
    public static ActivitySource ActivitySource { get; } = new(ActivitySourceName);
}
