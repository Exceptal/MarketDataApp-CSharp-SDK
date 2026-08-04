using MarketDataApp.Stocks;

namespace MarketDataApp;

/// <summary>Response for <c>stocks.Earnings()</c> — earnings history and forward estimates for a symbol.</summary>
public sealed record StockEarningsResponse : MarketDataResponse<IReadOnlyList<StockEarning>>;
