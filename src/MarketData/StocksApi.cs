using MarketData.Stocks;

namespace MarketData;

/// <summary>Asynchronous stock endpoints.</summary>
public sealed class StocksApi
{
    private readonly ApiClient _apiClient;

    internal StocksApi(ApiClient apiClient) => _apiClient = apiClient;

    /// <summary>Gets a real-time or historical quote for one stock symbol.</summary>
    public async Task<StockQuotesResponse> GetQuoteAsync(
        StockQuoteRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = RequestQuery.From(options);
        RequestQuery.Add(query, "extended", request.Extended?.ToString().ToLowerInvariant());
        RequestQuery.Add(query, "candle", request.Candle?.ToString().ToLowerInvariant());
        RequestQuery.Add(query, "52week", request.Week52?.ToString().ToLowerInvariant());

        var response = await _apiClient.GetAsync(
            $"stocks/quotes/{Uri.EscapeDataString(request.Symbol)}",
            true,
            query,
            cancellationToken).ConfigureAwait(false);
        var values = JsonResponseParser.Decode(
            response,
            root => JsonResponseParser.ReadParallelArray(
                root,
                row => new StockQuote(
                    row.String("symbol"),
                    row.Double("ask"),
                    row.Long("askSize"),
                    row.Double("bid"),
                    row.Long("bidSize"),
                    row.Double("mid"),
                    row.Double("last"),
                    row.Double("change"),
                    row.Double("changepct"),
                    row.Long("volume"),
                    row.Timestamp("updated"),
                    row.Double("o"),
                    row.Double("h"),
                    row.Double("l"),
                    row.Double("c"),
                    row.Double("52weekHigh"),
                    row.Double("52weekLow")),
                "symbol", "ask", "askSize", "bid", "bidSize", "mid", "last", "change",
                "changepct", "volume", "updated", "o", "h", "l", "c", "52weekHigh", "52weekLow"));
        return JsonResponseParser.CreateResponse<StockQuotesResponse, IReadOnlyList<StockQuote>>(response, values);
    }
}
