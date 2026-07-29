namespace MarketData.Markets;

/// <summary>
/// Parameters for <c>GET /v1/markets/status/</c>.
/// Returns market open/closed status for one or more dates. All parameters are optional;
/// omitting all returns today's status for the US market.
/// </summary>
public record MarketStatusRequest
{
    /// <summary>
    /// ISO 3166 two-letter country code of the exchange to check (default: <c>"US"</c>).
    /// </summary>
    public string? Country { get; init; }

    /// <summary>Return status for a single date only.</summary>
    public DateOnly? Date { get; init; }

    /// <summary>Start date (inclusive) of the status window.</summary>
    public DateOnly? From { get; init; }

    /// <summary>End date (inclusive) of the status window.</summary>
    public DateOnly? To { get; init; }

    /// <summary>Number of trading days to return, counting back from <see cref="To"/> (or today).</summary>
    public int? Countback { get; init; }
}
