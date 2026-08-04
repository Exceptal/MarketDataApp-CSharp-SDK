namespace MarketData.Stocks;

/// <summary>
/// A single OHLCV candle for a stock. All fields are nullable because the
/// <c>columns</c> universal parameter can project any column away, and the backend
/// maps NaN to <c>null</c> for illiquid or off-hours bars.
/// </summary>
/// <param name="Time">Candle open time (wire field: <c>t</c>).</param>
/// <param name="Open">Opening price (wire field: <c>o</c>).</param>
/// <param name="High">High price (wire field: <c>h</c>).</param>
/// <param name="Low">Low price (wire field: <c>l</c>).</param>
/// <param name="Close">Closing price (wire field: <c>c</c>).</param>
/// <param name="Volume">Volume traded (wire field: <c>v</c>).</param>
public record StockCandle(
    DateTimeOffset? Time,
    decimal? Open,
    decimal? High,
    decimal? Low,
    decimal? Close,
    long? Volume);
