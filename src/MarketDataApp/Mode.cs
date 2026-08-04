namespace MarketDataApp;

/// <summary>Data mode controlling whether live, delayed, or cached data is returned.</summary>
public enum Mode
{
    /// <summary>Real-time data (default).</summary>
    Live,

    /// <summary>Data delayed by the exchange-mandated period.</summary>
    Delayed,

    /// <summary>Previously cached data.</summary>
    Cached,
}

internal static class ModeExtensions
{
    internal static string ToWireValue(this Mode mode) => mode switch
    {
        Mode.Live => "live",
        Mode.Delayed => "delayed",
        Mode.Cached => "cached",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
    };
}
