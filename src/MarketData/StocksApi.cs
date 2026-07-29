using MarketData.Stocks;
using System.Text.Json;

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

    /// <summary>Gets last prices for multiple stock symbols in one request.</summary>
    public async Task<StockPricesResponse> GetPricesAsync(
        StockPricesRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = RequestQuery.From(options);
        RequestQuery.Add(query, "symbols", string.Join(",", request.Symbols));
        var response = await _apiClient.GetAsync("stocks/prices", true, query, cancellationToken)
            .ConfigureAwait(false);
        var values = JsonResponseParser.Decode(
            response,
            root => JsonResponseParser.ReadParallelArray(
                root,
                row => new StockPrice(
                    row.String("symbol"),
                    row.Double("mid"),
                    row.Double("change"),
                    row.Double("changepct"),
                    row.Timestamp("updated")),
                "symbol", "mid", "change", "changepct", "updated"));
        return JsonResponseParser.CreateResponse<StockPricesResponse, IReadOnlyList<StockPrice>>(response, values);
    }

    /// <summary>Gets quotes for multiple stock symbols in one request.</summary>
    public async Task<StockQuotesResponse> GetQuotesAsync(
        StockQuotesRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = RequestQuery.From(options);
        RequestQuery.Add(query, "symbols", string.Join(",", request.Symbols));
        AddBoolean(query, "extended", request.Extended);
        AddBoolean(query, "candle", request.Candle);
        AddBoolean(query, "52week", request.Week52);
        var response = await _apiClient.GetAsync("stocks/quotes", true, query, cancellationToken)
            .ConfigureAwait(false);
        var values = JsonResponseParser.Decode(response, ParseQuotes);
        return JsonResponseParser.CreateResponse<StockQuotesResponse, IReadOnlyList<StockQuote>>(response, values);
    }

    /// <summary>Gets OHLCV candles for a stock symbol.</summary>
    public async Task<StockCandlesResponse> GetCandlesAsync(
        StockCandlesRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = RequestQuery.From(options);
        AddDateWindow(query, request.Date, request.From, request.To, request.Countback);
        RequestQuery.Add(query, "exchange", request.Exchange);
        AddBoolean(query, "extended", request.Extended);
        RequestQuery.Add(query, "country", request.Country);
        AddBoolean(query, "adjustsplits", request.AdjustSplits);
        AddBoolean(query, "adjustdividends", request.AdjustDividends);
        var path = $"stocks/candles/{Uri.EscapeDataString(request.Resolution.WireValue)}/{Uri.EscapeDataString(request.Symbol)}";
        var response = await _apiClient.GetAsync(path, true, query, cancellationToken)
            .ConfigureAwait(false);
        var values = JsonResponseParser.Decode(
            response,
            root => JsonResponseParser.ReadParallelArray(
                root,
                row => new StockCandle(
                    row.Timestamp("t"),
                    row.Double("o"),
                    row.Double("h"),
                    row.Double("l"),
                    row.Double("c"),
                    row.Long("v")),
                "t", "o", "h", "l", "c", "v"));
        return JsonResponseParser.CreateResponse<StockCandlesResponse, IReadOnlyList<StockCandle>>(response, values);
    }

    /// <summary>Gets news articles for a stock symbol.</summary>
    public async Task<StockNewsResponse> GetNewsAsync(
        StockNewsRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (options?.Columns is { Count: > 0 })
        {
            throw new ArgumentException("Columns projection is not supported for typed news responses.", nameof(options));
        }

        var query = RequestQuery.From(options);
        AddDateWindow(query, request.Date, request.From, request.To, request.Countback);
        var response = await _apiClient.GetAsync(
            $"stocks/news/{Uri.EscapeDataString(request.Symbol)}",
            true,
            query,
            cancellationToken).ConfigureAwait(false);
        var result = JsonResponseParser.Decode(
            response,
            root =>
            {
                var articles = JsonResponseParser.ReadParallelArray(
                    root,
                    row => new StockNewsArticle(
                        row.String("symbol") ?? throw new JsonException("Missing symbol."),
                        row.String("headline") ?? throw new JsonException("Missing headline."),
                        row.String("content") ?? throw new JsonException("Missing content."),
                        row.String("source") ?? throw new JsonException("Missing source."),
                        row.Timestamp("publicationDate") ?? throw new JsonException("Missing publicationDate.")),
                    "symbol", "headline", "content", "source", "publicationDate");
                return (Articles: (IReadOnlyList<StockNewsArticle>)articles, Updated: JsonResponseParser.Timestamp(root, "updated"));
            });
        return JsonResponseParser.CreateResponse<StockNewsResponse, IReadOnlyList<StockNewsArticle>>(
            response,
            result.Articles,
            typedResponse =>
            {
                typedResponse.Updated = result.Updated;
                return typedResponse;
            });
    }

    /// <summary>Gets historical and forward earnings data for a stock symbol.</summary>
    public async Task<StockEarningsResponse> GetEarningsAsync(
        StockEarningsRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = RequestQuery.From(options);
        AddDateWindow(query, request.Date, request.From, request.To, request.Countback);
        RequestQuery.Add(query, "report", request.Report);
        var response = await _apiClient.GetAsync(
            $"stocks/earnings/{Uri.EscapeDataString(request.Symbol)}",
            true,
            query,
            cancellationToken).ConfigureAwait(false);
        var values = JsonResponseParser.Decode(
            response,
            root => JsonResponseParser.ReadParallelArray(
                root,
                row => new StockEarning(
                    row.String("symbol"),
                    ToInt(row.Long("fiscalYear")),
                    ToInt(row.Long("fiscalQuarter")),
                    row.Timestamp("date"),
                    row.Timestamp("reportDate"),
                    row.String("reportTime"),
                    row.String("currency"),
                    row.Double("reportedEPS"),
                    row.Double("estimatedEPS"),
                    row.Double("surpriseEPS"),
                    row.Double("surpriseEPSpct"),
                    row.Timestamp("updated")),
                "symbol", "fiscalYear", "fiscalQuarter", "date", "reportDate", "reportTime",
                "currency", "reportedEPS", "estimatedEPS", "surpriseEPS", "surpriseEPSpct", "updated"));
        return JsonResponseParser.CreateResponse<StockEarningsResponse, IReadOnlyList<StockEarning>>(response, values);
    }

    private static IReadOnlyList<StockQuote> ParseQuotes(System.Text.Json.JsonElement root) =>
        JsonResponseParser.ReadParallelArray(
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
            "changepct", "volume", "updated", "o", "h", "l", "c", "52weekHigh", "52weekLow");

    private static void AddDateWindow(
        ICollection<KeyValuePair<string, string?>> query,
        DateOnly? date,
        DateOnly? from,
        DateOnly? to,
        int? countback)
    {
        RequestQuery.Add(query, "date", date is { } dateValue ? RequestQuery.Date(dateValue) : null);
        RequestQuery.Add(query, "from", from is { } fromValue ? RequestQuery.Date(fromValue) : null);
        RequestQuery.Add(query, "to", to is { } toValue ? RequestQuery.Date(toValue) : null);
        RequestQuery.Add(query, "countback", countback?.ToString());
    }

    private static void AddBoolean(
        ICollection<KeyValuePair<string, string?>> query,
        string name,
        bool? value) =>
        RequestQuery.Add(query, name, value?.ToString().ToLowerInvariant());

    private static int? ToInt(long? value) => value is null ? null : checked((int)value.Value);
}
