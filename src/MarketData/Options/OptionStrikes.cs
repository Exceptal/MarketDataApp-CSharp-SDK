namespace MarketData.Options;

/// <summary>Available strike prices grouped by expiration date.</summary>
/// <param name="Updated">Timestamp when the strike data was last updated.</param>
/// <param name="ByExpiration">Strike prices keyed by expiration date.</param>
public sealed record OptionStrikes(
    DateTimeOffset? Updated,
    IReadOnlyDictionary<DateOnly, IReadOnlyList<decimal>> ByExpiration);
