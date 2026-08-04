namespace MarketData.Stocks;

/// <summary>Last price snapshot for a stock symbol — a lightweight alternative to a full quote.</summary>
/// <param name="Symbol">Ticker symbol.</param>
/// <param name="Mid">Last mid price.</param>
/// <param name="Change">Absolute price change vs the prior close.</param>
/// <param name="ChangePct">Fractional price change vs the prior close (wire field: <c>changepct</c>).</param>
/// <param name="Updated">Timestamp of the price (America/New_York).</param>
public record StockPrice(
    string? Symbol,
    decimal? Mid,
    decimal? Change,
    double? ChangePct,
    DateTimeOffset? Updated);
