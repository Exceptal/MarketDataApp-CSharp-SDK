using System.Text.Json;
using MarketData.Markets;

namespace MarketData;

/// <summary>Asynchronous market-calendar endpoints.</summary>
public sealed class MarketsApi
{
    private readonly ApiClient _apiClient;

    internal MarketsApi(ApiClient apiClient) => _apiClient = apiClient;

    /// <summary>Gets exchange open/closed status for the requested dates.</summary>
    public async Task<MarketStatusResponse> GetStatusAsync(
        MarketStatusRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);
        var query = RequestQuery.From(options);
        RequestQuery.Add(query, "country", request.Country);
        RequestQuery.AddDateWindow(query, request.Date, request.From, request.To, request.Countback);

        var response = await _apiClient.GetAsync("markets/status", true, query, cancellationToken)
            .ConfigureAwait(false);
        var values = JsonResponseParser.DecodeOrDefault(
            response,
            root => JsonResponseParser.ReadParallelArray(
                root,
                row => new MarketStatus(row.Timestamp("date"), row.String("status")),
                "date", "status"),
            Array.Empty<MarketStatus>(),
            requestedColumns: options?.Columns);
        return JsonResponseParser.CreateResponse<MarketStatusResponse, IReadOnlyList<MarketStatus>>(response, values);
    }

    /// <summary>Gets exchange open/closed status as CSV.</summary>
    public async Task<CsvResponse> GetStatusCsvAsync(
        MarketStatusRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);
        var query = RequestQuery.Csv(options);
        RequestQuery.Add(query, "country", request.Country);
        RequestQuery.AddDateWindow(query, request.Date, request.From, request.To, request.Countback);
        var response = await _apiClient.GetAsync("markets/status", true, query, cancellationToken)
            .ConfigureAwait(false);
        return JsonResponseParser.CreateCsvResponse(response);
    }

    private static void ValidateRequest(MarketStatusRequest request)
    {
        RequestValidator.ValidateDateWindow(
            request.Date, request.From, request.To, request.Countback, nameof(request));
        if (request.Country is { Length: not 2 })
        {
            throw new ArgumentException(
                "Country must be a two-letter ISO 3166 country code.",
                nameof(request));
        }
    }
}
