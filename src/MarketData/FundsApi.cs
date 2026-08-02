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
        RequestValidator.ValidateDateWindow(
            request.Date, request.From, request.To, request.Countback, nameof(request));
        var query = RequestQuery.From(options);
        RequestQuery.AddDateWindow(query, request.Date, request.From, request.To, request.Countback);
        var path =
            $"funds/candles/{Uri.EscapeDataString(request.Resolution.WireValue)}/{Uri.EscapeDataString(request.Symbol)}";

        var response = await _apiClient.GetAsync(path, true, query, cancellationToken)
            .ConfigureAwait(false);
        var values = JsonResponseParser.DecodeOrDefault(
            response,
            root => JsonResponseParser.ReadParallelArray(
                root,
                row => new FundCandle(
                    row.Timestamp("t"),
                    row.Double("o"),
                    row.Double("h"),
                    row.Double("l"),
                    row.Double("c")),
                "t", "o", "h", "l", "c"),
            Array.Empty<FundCandle>(),
            requestedColumns: options?.Columns);
        return JsonResponseParser.CreateResponse<FundCandlesResponse, IReadOnlyList<FundCandle>>(response, values);
    }

    /// <summary>Gets CSV OHLC candles for a fund or ETF symbol.</summary>
    public async Task<CsvResponse> GetCandlesCsvAsync(
        FundCandlesRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequestValidator.ValidateDateWindow(
            request.Date, request.From, request.To, request.Countback, nameof(request));
        var query = RequestQuery.Csv(options);
        RequestQuery.AddDateWindow(query, request.Date, request.From, request.To, request.Countback);
        var path =
            $"funds/candles/{Uri.EscapeDataString(request.Resolution.WireValue)}/{Uri.EscapeDataString(request.Symbol)}";
        var response = await _apiClient.GetAsync(path, true, query, cancellationToken)
            .ConfigureAwait(false);
        return JsonResponseParser.CreateCsvResponse(response);
    }

}
