using MarketData.Stocks;

namespace MarketData;

/// <summary>Response for <c>stocks.Candles()</c> — OHLCV candle series for a single symbol.</summary>
public sealed record StockCandlesResponse : MarketDataResponse<IReadOnlyList<StockCandle>>;
