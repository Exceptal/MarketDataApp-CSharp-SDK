namespace MarketData.Stocks;

/// <summary>Parameters for <c>GET /v1/stocks/prices/{symbol}/</c>.</summary>
public sealed record StockPriceRequest
{
    /// <summary>Ticker symbol.</summary>
    public string Symbol { get; init; }

    /// <summary>Initializes a request for <paramref name="symbol"/>.</summary>
    public StockPriceRequest(string symbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        Symbol = symbol;
    }
}
