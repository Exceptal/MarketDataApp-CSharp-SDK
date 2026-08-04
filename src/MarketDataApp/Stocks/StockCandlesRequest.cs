namespace MarketDataApp.Stocks;

/// <summary>
/// Parameters for <c>GET /v1/stocks/candles/{resolution}/{symbol}/</c>.
/// </summary>
/// <remarks>
/// <see cref="Date"/> cannot be combined with range or countback fields.
/// <see cref="Countback"/> may be combined with <see cref="To"/>, but not <see cref="From"/>.
/// Validation is enforced by the resource façade before the request is dispatched.
/// </remarks>
public record StockCandlesRequest
{
    /// <summary>Candle resolution.</summary>
    public StockResolution Resolution { get; init; }

    /// <summary>Ticker symbol.</summary>
    public string Symbol { get; init; }

    /// <summary>Return candles for a single date only.</summary>
    public DateOnly? Date { get; init; }

    /// <summary>Start date (inclusive) of the date range.</summary>
    public DateOnly? From { get; init; }

    /// <summary>End date (inclusive) of the date range.</summary>
    public DateOnly? To { get; init; }

    /// <summary>Number of candles to return, counting back from <see cref="To"/> (or today).</summary>
    public int? Countback { get; init; }

    /// <summary>Limit results to a specific exchange (e.g. <c>"NASDAQ"</c>).</summary>
    public string? Exchange { get; init; }

    /// <summary>Include extended-hours candles (pre/post market). API default: <c>true</c>.</summary>
    public bool? Extended { get; init; }

    /// <summary>ISO 3166 two-letter country code for the exchange (default: <c>"US"</c>).</summary>
    public string? Country { get; init; }

    /// <summary>Adjust prices for stock splits.</summary>
    public bool? AdjustSplits { get; init; }

    /// <summary>Adjust prices for dividends.</summary>
    public bool? AdjustDividends { get; init; }

    /// <summary>Initializes the request with the required <paramref name="resolution"/> and <paramref name="symbol"/>.</summary>
    public StockCandlesRequest(StockResolution resolution, string symbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        Resolution = resolution;
        Symbol = symbol;
    }
}
