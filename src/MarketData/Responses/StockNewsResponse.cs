using MarketData.Stocks;

namespace MarketData;

/// <summary>Response for <c>stocks.News()</c> — news articles for a single symbol.</summary>
public sealed record StockNewsResponse : MarketDataResponse<IReadOnlyList<StockNewsArticle>>
{
    /// <summary>
    /// Timestamp of the most recent article in the response, or <c>null</c> for historical queries.
    /// </summary>
    public DateTimeOffset? Updated { get; internal init; }
}
