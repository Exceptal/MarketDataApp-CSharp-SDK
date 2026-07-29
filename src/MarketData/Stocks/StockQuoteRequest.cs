namespace MarketData.Stocks;

/// <summary>
/// Parameters for the single-symbol stock quote endpoint
/// <c>GET /v1/stocks/quotes/{symbol}/</c>.
/// For multiple symbols in one call use <see cref="StockQuotesRequest"/>.
/// </summary>
public record StockQuoteRequest
{
    /// <summary>Ticker symbol.</summary>
    public string Symbol { get; init; }

    /// <summary>Include extended-hours prices. API default: <c>true</c>.</summary>
    public bool? Extended { get; init; }

    /// <summary>Append OHLC columns (Open/High/Low/Close) to the response row.</summary>
    public bool? Candle { get; init; }

    /// <summary>Append 52-week high/low columns to the response row.</summary>
    public bool? Week52 { get; init; }

    /// <summary>Initializes the request with the required <paramref name="symbol"/>.</summary>
    public StockQuoteRequest(string symbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        Symbol = symbol;
    }
}
