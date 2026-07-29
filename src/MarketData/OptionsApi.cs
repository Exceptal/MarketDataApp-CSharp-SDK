using System.Globalization;
using System.Text.Json;
using MarketData.Options;

namespace MarketData;

/// <summary>Asynchronous options endpoints.</summary>
public sealed class OptionsApi
{
    private readonly ApiClient _apiClient;

    internal OptionsApi(ApiClient apiClient) => _apiClient = apiClient;

    /// <summary>Resolves user input to a canonical OCC option symbol.</summary>
    public async Task<OptionsLookupResponse> GetLookupAsync(
        OptionsLookupRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var response = await _apiClient.GetAsync(
            $"options/lookup/{Uri.EscapeDataString(request.UserInput)}",
            true,
            RequestQuery.From(options),
            cancellationToken).ConfigureAwait(false);
        var value = JsonResponseParser.Decode(
            response,
            root => root.TryGetProperty("optionSymbol", out var symbol)
                && symbol.ValueKind == JsonValueKind.String
                ? symbol.GetString() ?? throw new JsonException("Missing optionSymbol.")
                : throw new JsonException("Missing optionSymbol."));
        return JsonResponseParser.CreateResponse<OptionsLookupResponse, string>(response, value);
    }

    /// <summary>Gets available expiration dates for an underlying symbol.</summary>
    public async Task<OptionsExpirationsResponse> GetExpirationsAsync(
        OptionsExpirationsRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = RequestQuery.From(options);
        Add(query, "strike", request.Strike);
        AddDate(query, "date", request.Date);
        var response = await _apiClient.GetAsync(
            $"options/expirations/{Uri.EscapeDataString(request.Symbol)}",
            true,
            query,
            cancellationToken).ConfigureAwait(false);
        var result = JsonResponseParser.Decode(
            response,
            root =>
            {
                var expirations = JsonResponseParser.ReadParallelArray(
                    root,
                    row => row.Timestamp("expiration")
                        ?? throw new JsonException("Missing expiration."),
                    "expiration");
                return (Values: expirations, Updated: JsonResponseParser.Timestamp(root, "updated"));
            });
        return JsonResponseParser.CreateResponse<OptionsExpirationsResponse, IReadOnlyList<DateTimeOffset>>(
            response,
            result.Values,
            typedResponse =>
            {
                typedResponse.Updated = result.Updated;
                return typedResponse;
            });
    }

    /// <summary>Gets historical or current quotes for one OCC option symbol.</summary>
    public async Task<OptionsQuotesResponse> GetQuoteAsync(
        OptionsQuoteRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var response = await GetQuoteResponseAsync(
            request.OptionSymbol,
            request.Date,
            request.From,
            request.To,
            request.Countback,
            options,
            cancellationToken).ConfigureAwait(false);
        return response;
    }

    /// <summary>
    /// Gets quotes for multiple OCC option symbols. The API is called once per symbol concurrently.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, OptionsQuotesResponse>> GetQuotesAsync(
        OptionsQuotesRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var tasks = request.OptionSymbols.Select(async symbol =>
        {
            var response = await GetQuoteResponseAsync(
                symbol,
                request.Date,
                request.From,
                request.To,
                request.Countback,
                options,
                cancellationToken).ConfigureAwait(false);
            return (symbol, response);
        });
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.ToDictionary(item => item.symbol, item => item.response);
    }

    /// <summary>Gets the options chain for an underlying symbol.</summary>
    public async Task<OptionsChainResponse> GetChainAsync(
        OptionsChainRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = RequestQuery.From(options);
        AddExpirationFilter(query, request.Expiration);
        AddBoolean(query, "weekly", request.Weekly);
        AddBoolean(query, "monthly", request.Monthly);
        AddBoolean(query, "quarterly", request.Quarterly);
        AddBoolean(query, "am", request.Am);
        AddBoolean(query, "pm", request.Pm);
        AddBoolean(query, "nonstandard", request.NonStandard);
        RequestQuery.Add(query, "strike", FormatStrikeFilter(request.Strike));
        Add(query, "delta", request.Delta);
        Add(query, "strikeLimit", request.StrikeLimit);
        RequestQuery.Add(query, "range", request.StrikeRangeFilter?.ToWireValue());
        Add(query, "minBid", request.MinBid);
        Add(query, "maxBid", request.MaxBid);
        Add(query, "minAsk", request.MinAsk);
        Add(query, "maxAsk", request.MaxAsk);
        Add(query, "maxBidAskSpread", request.MaxBidAskSpread);
        Add(query, "maxBidAskSpreadPct", request.MaxBidAskSpreadPct);
        Add(query, "minOpenInterest", request.MinOpenInterest);
        Add(query, "minVolume", request.MinVolume);
        RequestQuery.Add(query, "side", request.Side?.ToWireValue());
        AddDate(query, "date", request.Date);

        var response = await _apiClient.GetAsync(
            $"options/chain/{Uri.EscapeDataString(request.Symbol)}",
            true,
            query,
            cancellationToken).ConfigureAwait(false);
        var values = JsonResponseParser.Decode(response, ParseOptionQuotes);
        return JsonResponseParser.CreateResponse<OptionsChainResponse, IReadOnlyList<OptionQuote>>(response, values);
    }

    private async Task<OptionsQuotesResponse> GetQuoteResponseAsync(
        string optionSymbol,
        DateOnly? date,
        DateOnly? from,
        DateOnly? to,
        int? countback,
        MarketDataRequestOptions? options,
        CancellationToken cancellationToken)
    {
        var query = RequestQuery.From(options);
        AddDateWindow(query, date, from, to, countback);
        var response = await _apiClient.GetAsync(
            $"options/quotes/{Uri.EscapeDataString(optionSymbol)}",
            true,
            query,
            cancellationToken).ConfigureAwait(false);
        var values = JsonResponseParser.Decode(response, ParseOptionQuotes);
        return JsonResponseParser.CreateResponse<OptionsQuotesResponse, IReadOnlyList<OptionQuote>>(response, values);
    }

    private static IReadOnlyList<OptionQuote> ParseOptionQuotes(JsonElement root) =>
        JsonResponseParser.ReadParallelArray(
            root,
            row => new OptionQuote(
                row.String("optionSymbol"),
                row.String("underlying"),
                row.Timestamp("expiration"),
                row.String("side"),
                row.Double("strike"),
                row.Timestamp("firstTraded"),
                ToInt(row.Long("dte")),
                row.Timestamp("updated"),
                row.Double("bid"),
                row.Long("bidSize"),
                row.Double("mid"),
                row.Double("ask"),
                row.Long("askSize"),
                row.Double("last"),
                row.Long("openInterest"),
                row.Long("volume"),
                row.Boolean("inTheMoney"),
                row.Double("intrinsicValue"),
                row.Double("extrinsicValue"),
                row.Double("underlyingPrice"),
                row.Double("iv"),
                row.Double("delta"),
                row.Double("gamma"),
                row.Double("theta"),
                row.Double("vega"),
                row.Double("rho")),
            "optionSymbol", "underlying", "expiration", "side", "strike", "firstTraded", "dte",
            "updated", "bid", "bidSize", "mid", "ask", "askSize", "last", "openInterest",
            "volume", "inTheMoney", "intrinsicValue", "extrinsicValue", "underlyingPrice",
            "iv", "delta", "gamma", "theta", "vega", "rho");

    private static void AddExpirationFilter(
        ICollection<KeyValuePair<string, string?>> query,
        ExpirationFilter? filter)
    {
        switch (filter)
        {
            case ExpirationFilter.OnDate onDate:
                AddDate(query, "expiration", onDate.Date);
                break;
            case ExpirationFilter.Dte dte:
                Add(query, "dte", dte.Days);
                break;
            case ExpirationFilter.Between between:
                AddDate(query, "from", between.From);
                AddDate(query, "to", between.To);
                break;
            case ExpirationFilter.MonthYear monthYear:
                Add(query, "month", monthYear.Month);
                Add(query, "year", monthYear.Year);
                break;
            case ExpirationFilter.All:
                RequestQuery.Add(query, "expiration", "all");
                break;
        }
    }

    private static string? FormatStrikeFilter(StrikeFilter? filter) =>
        filter switch
        {
            null => null,
            StrikeFilter.Exact exact => FormatNumber(exact.Price),
            StrikeFilter.Range range => $"{FormatNumber(range.Min)}-{FormatNumber(range.Max)}",
            StrikeFilter.Comparison comparison =>
                $"{comparison.Op.OperatorWireValue()}{FormatNumber(comparison.Price)}",
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };

    private static void AddDateWindow(
        ICollection<KeyValuePair<string, string?>> query,
        DateOnly? date,
        DateOnly? from,
        DateOnly? to,
        int? countback)
    {
        AddDate(query, "date", date);
        AddDate(query, "from", from);
        AddDate(query, "to", to);
        Add(query, "countback", countback);
    }

    private static void AddDate(
        ICollection<KeyValuePair<string, string?>> query,
        string name,
        DateOnly? value) =>
        RequestQuery.Add(query, name, value is { } date ? RequestQuery.Date(date) : null);

    private static void AddBoolean(
        ICollection<KeyValuePair<string, string?>> query,
        string name,
        bool? value) =>
        RequestQuery.Add(query, name, value?.ToString().ToLowerInvariant());

    private static void Add(
        ICollection<KeyValuePair<string, string?>> query,
        string name,
        int? value) =>
        RequestQuery.Add(query, name, value?.ToString(CultureInfo.InvariantCulture));

    private static void Add(
        ICollection<KeyValuePair<string, string?>> query,
        string name,
        long? value) =>
        RequestQuery.Add(query, name, value?.ToString(CultureInfo.InvariantCulture));

    private static void Add(
        ICollection<KeyValuePair<string, string?>> query,
        string name,
        double? value) =>
        RequestQuery.Add(query, name, value?.ToString(CultureInfo.InvariantCulture));

    private static string FormatNumber(double value) =>
        value.ToString("G", CultureInfo.InvariantCulture);

    private static int? ToInt(long? value) => value is null ? null : checked((int)value.Value);
}
