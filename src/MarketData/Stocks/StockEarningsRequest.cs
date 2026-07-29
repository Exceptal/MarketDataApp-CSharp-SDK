namespace MarketData.Stocks;

/// <summary>
/// Parameters for <c>GET /v1/stocks/earnings/{symbol}/</c>.
/// Returns historical earnings reports and forward earnings estimates.
/// </summary>
public record StockEarningsRequest
{
    /// <summary>Ticker symbol.</summary>
    public string Symbol { get; init; }

    /// <summary>Return earnings for a single fiscal-period end date.</summary>
    public DateOnly? Date { get; init; }

    /// <summary>Start date (inclusive) of the earnings history window.</summary>
    public DateOnly? From { get; init; }

    /// <summary>End date (inclusive) of the earnings history window.</summary>
    public DateOnly? To { get; init; }

    /// <summary>Number of earnings periods to return, counting back from <see cref="To"/> (or today).</summary>
    public int? Countback { get; init; }

    /// <summary>
    /// Return only a specific report period (e.g. <c>"2024-Q4"</c>).
    /// When set, <see cref="Date"/> / <see cref="From"/> / <see cref="To"/> / <see cref="Countback"/>
    /// are ignored.
    /// </summary>
    public string? Report { get; init; }

    /// <summary>Initializes the request with the required <paramref name="symbol"/>.</summary>
    public StockEarningsRequest(string symbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        Symbol = symbol;
    }
}
