using MarketData.Stocks;

namespace MarketData;

/// <summary>Response for <c>stocks.Prices()</c> — last price snapshot per symbol.</summary>
public sealed record StockPricesResponse : MarketDataResponse<IReadOnlyList<StockPrice>>;
