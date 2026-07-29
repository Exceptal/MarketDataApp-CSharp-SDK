namespace MarketData.Funds;

/// <summary>
/// Parameters for <c>GET /v1/funds/candles/{resolution}/{symbol}/</c>.
/// Intraday resolutions are not supported — use <see cref="FundResolution.Daily"/> or longer.
/// </summary>
public record FundCandlesRequest
{
    /// <summary>Candle resolution (daily or longer; intraday is not supported for funds).</summary>
    public FundResolution Resolution { get; init; }

    /// <summary>Fund or ETF ticker symbol.</summary>
    public string Symbol { get; init; }

    /// <summary>Return candles for a single date only.</summary>
    public DateOnly? Date { get; init; }

    /// <summary>Start date (inclusive) of the date range.</summary>
    public DateOnly? From { get; init; }

    /// <summary>End date (inclusive) of the date range.</summary>
    public DateOnly? To { get; init; }

    /// <summary>Number of candles to return, counting back from <see cref="To"/> (or today).</summary>
    public int? Countback { get; init; }

    /// <summary>Initializes the request with the required <paramref name="resolution"/> and <paramref name="symbol"/>.</summary>
    public FundCandlesRequest(FundResolution resolution, string symbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        Resolution = resolution;
        Symbol = symbol;
    }
}
