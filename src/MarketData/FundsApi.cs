using MarketData.Funds;

namespace MarketData;

/// <summary>Asynchronous fund and ETF endpoints.</summary>
public sealed class FundsApi
{
    private readonly ApiClient _apiClient;

    internal FundsApi(ApiClient apiClient) => _apiClient = apiClient;

    /// <summary>Gets OHLC candles for a fund or ETF symbol.</summary>
    public async Task<FundCandlesResponse> GetCandlesAsync(
        FundCandlesRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = RequestQuery.From(options);
        AddDateWindow(query, request.Date, request.From, request.To, request.Countback);
        var path =
            $"funds/candles/{Uri.EscapeDataString(request.Resolution.WireValue)}/{Uri.EscapeDataString(request.Symbol)}";

        var response = await _apiClient.GetAsync(path, true, query, cancellationToken)
            .ConfigureAwait(false);
        var values = JsonResponseParser.Decode(
            response,
            root => JsonResponseParser.ReadParallelArray(
                root,
                row => new FundCandle(
                    row.Timestamp("t"),
                    row.Double("o"),
                    row.Double("h"),
                    row.Double("l"),
                    row.Double("c")),
                "t", "o", "h", "l", "c"));
        return JsonResponseParser.CreateResponse<FundCandlesResponse, IReadOnlyList<FundCandle>>(response, values);
    }

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
}
