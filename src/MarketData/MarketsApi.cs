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
        var query = RequestQuery.From(options);
        RequestQuery.Add(query, "country", request.Country);
        RequestQuery.Add(query, "date", request.Date is { } date ? RequestQuery.Date(date) : null);
        RequestQuery.Add(query, "from", request.From is { } from ? RequestQuery.Date(from) : null);
        RequestQuery.Add(query, "to", request.To is { } to ? RequestQuery.Date(to) : null);
        RequestQuery.Add(query, "countback", request.Countback?.ToString());

        var response = await _apiClient.GetAsync("markets/status", true, query, cancellationToken)
            .ConfigureAwait(false);
        var values = JsonResponseParser.DecodeOrDefault(
            response,
            root => JsonResponseParser.ReadParallelArray(
                root,
                row => new MarketStatus(row.Timestamp("date"), row.String("status")),
                "date", "status"),
            Array.Empty<MarketStatus>());
        return JsonResponseParser.CreateResponse<MarketStatusResponse, IReadOnlyList<MarketStatus>>(response, values);
    }

    /// <summary>Gets exchange open/closed status as CSV.</summary>
    public async Task<CsvResponse> GetStatusCsvAsync(
        MarketStatusRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = RequestQuery.Csv(options);
        RequestQuery.Add(query, "country", request.Country);
        RequestQuery.Add(query, "date", request.Date is { } date ? RequestQuery.Date(date) : null);
        RequestQuery.Add(query, "from", request.From is { } from ? RequestQuery.Date(from) : null);
        RequestQuery.Add(query, "to", request.To is { } to ? RequestQuery.Date(to) : null);
        RequestQuery.Add(query, "countback", request.Countback?.ToString());
        var response = await _apiClient.GetAsync("markets/status", true, query, cancellationToken)
            .ConfigureAwait(false);
        return JsonResponseParser.CreateCsvResponse(response);
    }
}
