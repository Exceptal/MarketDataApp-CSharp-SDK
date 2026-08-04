namespace MarketDataApp.Options;

/// <summary>
/// Moneyness filter for an options chain request.
/// </summary>
public enum StrikeRange
{
    /// <summary>In-the-money contracts only.</summary>
    Itm,

    /// <summary>Out-of-the-money contracts only.</summary>
    Otm,

    /// <summary>All contracts regardless of moneyness.</summary>
    All,
}

internal static class StrikeRangeExtensions
{
    internal static string ToWireValue(this StrikeRange range) => range switch
    {
        StrikeRange.Itm => "itm",
        StrikeRange.Otm => "otm",
        StrikeRange.All => "all",
        _ => throw new ArgumentOutOfRangeException(nameof(range), range, null),
    };
}
