namespace MarketData;

/// <summary>
/// Response for <c>options.Expirations()</c> — all available expiration dates for an underlying.
/// </summary>
public sealed record OptionsExpirationsResponse : MarketDataResponse<IReadOnlyList<DateTimeOffset>>
{
    /// <summary>Timestamp of the last data update, or <c>null</c> on a no-data response.</summary>
    public DateTimeOffset? Updated { get; internal set; }
}
