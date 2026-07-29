namespace MarketData.Stocks;

/// <summary>
/// Parameters for the multi-symbol last-price endpoint
/// <c>GET /v1/stocks/prices/?symbols=A,B,C</c>.
/// Returns a lightweight price-only row per symbol with no OHLC or book data.
/// </summary>
public record StockPricesRequest
{
    /// <summary>Ticker symbols to price (at least one required).</summary>
    public IReadOnlyList<string> Symbols { get; init; }

    /// <summary>Initializes the request with one or more ticker symbols.</summary>
    public StockPricesRequest(params string[] symbols) : this((IEnumerable<string>)symbols) { }

    /// <summary>Initializes the request from a sequence of ticker symbols.</summary>
    public StockPricesRequest(IEnumerable<string> symbols)
    {
        var list = (symbols ?? throw new ArgumentNullException(nameof(symbols))).ToList();
        if (list.Count == 0) throw new ArgumentException("At least one symbol is required.", nameof(symbols));
        if (list.Exists(string.IsNullOrWhiteSpace))
            throw new ArgumentException("All symbols must be non-empty strings.", nameof(symbols));
        Symbols = list.AsReadOnly();
    }
}
