using MarketData.Stocks;

namespace MarketData;

/// <summary>Response for <c>stocks.Earnings()</c> — earnings history and forward estimates for a symbol.</summary>
public sealed record StockEarningsResponse : MarketDataResponse<IReadOnlyList<StockEarning>>;
