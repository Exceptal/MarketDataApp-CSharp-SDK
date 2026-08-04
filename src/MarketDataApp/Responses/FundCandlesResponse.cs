using MarketDataApp.Funds;

namespace MarketDataApp;

/// <summary>Response for <c>funds.Candles()</c> — OHLC candle series for a fund or ETF.</summary>
public sealed record FundCandlesResponse : MarketDataResponse<IReadOnlyList<FundCandle>>;
