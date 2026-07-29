using MarketData.Stocks;

namespace MarketData;

/// <summary>
/// Response for <c>stocks.Quote()</c> and <c>stocks.Quotes()</c>.
/// <see cref="MarketDataResponse{T}.Values"/> contains one row for the single-symbol form
/// and one row per symbol for the batch form.
/// </summary>
public sealed record StockQuotesResponse : MarketDataResponse<IReadOnlyList<StockQuote>>;
