namespace MarketDataApp.Stocks;

/// <summary>
/// Parameters for the multi-symbol stock quotes endpoint
/// <c>GET /v1/stocks/quotes/?symbols=A,B,C</c>.
/// The backend batches all symbols into a single request — no fan-out.
/// For a single symbol use <see cref="StockQuoteRequest"/>.
/// </summary>
public record StockQuotesRequest
{
    /// <summary>Ticker symbols to quote (at least one required).</summary>
    public IReadOnlyList<string> Symbols { get; init; }

    /// <summary>Include extended-hours prices. API default: <c>true</c>.</summary>
    public bool? Extended { get; init; }

    /// <summary>Append OHLC columns to every response row.</summary>
    public bool? Candle { get; init; }

    /// <summary>Append 52-week high/low columns to every response row.</summary>
    public bool? Week52 { get; init; }

    /// <summary>Initializes the request with one or more ticker symbols.</summary>
    public StockQuotesRequest(params string[] symbols) : this((IEnumerable<string>)symbols) { }

    /// <summary>Initializes the request from a sequence of ticker symbols.</summary>
    public StockQuotesRequest(IEnumerable<string> symbols)
    {
        var list = (symbols ?? throw new ArgumentNullException(nameof(symbols))).ToList();
        if (list.Count == 0) throw new ArgumentException("At least one symbol is required.", nameof(symbols));
        if (list.Exists(string.IsNullOrWhiteSpace))
            throw new ArgumentException("All symbols must be non-empty strings.", nameof(symbols));
        Symbols = list.AsReadOnly();
    }
}
