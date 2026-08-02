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
        var value = JsonResponseParser.DecodeOrDefault(
            response,
            root => root.TryGetProperty("optionSymbol", out var symbol)
                && symbol.ValueKind == JsonValueKind.String
                ? symbol.GetString() ?? throw new JsonException("Missing optionSymbol.")
                : throw new JsonException("Missing optionSymbol."),
            string.Empty,
            requestedColumns: options?.Columns);
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
        AddBoolean(query, "nonstandard", request.NonStandard);
        var response = await _apiClient.GetAsync(
            $"options/expirations/{Uri.EscapeDataString(request.Symbol)}",
            true,
            query,
            cancellationToken).ConfigureAwait(false);
        var result = JsonResponseParser.DecodeOrDefault(
            response,
            root =>
            {
                var expirations = JsonResponseParser.ReadParallelArray(
                    root,
                    row => row.Timestamp("expirations")
                        ?? throw new JsonException("Missing expirations."),
                    "expirations");
                return (Values: expirations, Updated: JsonResponseParser.Timestamp(root, "updated"));
            },
            (Values: (IReadOnlyList<DateTimeOffset>)Array.Empty<DateTimeOffset>(), Updated: (DateTimeOffset?)null),
            requestedColumns: options?.Columns);
        return JsonResponseParser.CreateResponse<OptionsExpirationsResponse, IReadOnlyList<DateTimeOffset>>(
            response,
            result.Values,
            typedResponse =>
            {
                typedResponse.Updated = result.Updated;
                return typedResponse;
            });
    }

    /// <summary>Gets available strike prices grouped by expiration date.</summary>
    public async Task<OptionsStrikesResponse> GetStrikesAsync(
        OptionsStrikesRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = RequestQuery.From(options);
        AddDate(query, "date", request.Date);
        AddDate(query, "expiration", request.Expiration);
        var response = await _apiClient.GetAsync(
            $"options/strikes/{Uri.EscapeDataString(request.Underlying)}",
            true,
            query,
            cancellationToken).ConfigureAwait(false);
        var values = JsonResponseParser.DecodeOrDefault(
            response,
            ParseOptionStrikes,
            new OptionStrikes(
                null,
                new Dictionary<DateOnly, IReadOnlyList<double>>()),
            requestedColumns: options?.Columns);
        return JsonResponseParser.CreateResponse<OptionsStrikesResponse, OptionStrikes>(response, values);
    }

    /// <summary>Gets historical or current quotes for one OCC option symbol.</summary>
    public async Task<OptionsQuotesResponse> GetQuoteAsync(
        OptionsQuoteRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequestValidator.ValidateDateWindow(
            request.Date, request.From, request.To, request.Countback, nameof(request));
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
        RequestValidator.ValidateDateWindow(
            request.Date, request.From, request.To, request.Countback, nameof(request));
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
        ValidateChainRequest(request);
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
        var values = JsonResponseParser.DecodeOrDefault(
            response,
            ParseOptionQuotes,
            Array.Empty<OptionQuote>(),
            requestedColumns: options?.Columns);
        return JsonResponseParser.CreateResponse<OptionsChainResponse, IReadOnlyList<OptionQuote>>(response, values);
    }

    /// <summary>Resolves user input to a canonical OCC option symbol as CSV.</summary>
    public async Task<CsvResponse> GetLookupCsvAsync(
        OptionsLookupRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await GetCsvAsync(
            $"options/lookup/{Uri.EscapeDataString(request.UserInput)}",
            RequestQuery.Csv(options),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Gets available expiration dates for an underlying symbol as CSV.</summary>
    public async Task<CsvResponse> GetExpirationsCsvAsync(
        OptionsExpirationsRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = RequestQuery.Csv(options);
        Add(query, "strike", request.Strike);
        AddDate(query, "date", request.Date);
        AddBoolean(query, "nonstandard", request.NonStandard);
        return await GetCsvAsync(
            $"options/expirations/{Uri.EscapeDataString(request.Symbol)}", query, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Gets available strike prices grouped by expiration date as CSV.</summary>
    public async Task<CsvResponse> GetStrikesCsvAsync(
        OptionsStrikesRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = RequestQuery.Csv(options);
        AddDate(query, "date", request.Date);
        AddDate(query, "expiration", request.Expiration);
        return await GetCsvAsync(
            $"options/strikes/{Uri.EscapeDataString(request.Underlying)}",
            query,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Gets historical or current quotes for one OCC option symbol as CSV.</summary>
    public async Task<CsvResponse> GetQuoteCsvAsync(
        OptionsQuoteRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequestValidator.ValidateDateWindow(
            request.Date, request.From, request.To, request.Countback, nameof(request));
        var query = RequestQuery.Csv(options);
        RequestQuery.AddDateWindow(query, request.Date, request.From, request.To, request.Countback);
        return await GetCsvAsync(
            $"options/quotes/{Uri.EscapeDataString(request.OptionSymbol)}", query, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Gets quotes for multiple OCC option symbols as CSV, one response per symbol.</summary>
    public async Task<IReadOnlyDictionary<string, CsvResponse>> GetQuotesCsvAsync(
        OptionsQuotesRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequestValidator.ValidateDateWindow(
            request.Date, request.From, request.To, request.Countback, nameof(request));
        var tasks = request.OptionSymbols.Select(async symbol =>
        {
            var query = RequestQuery.Csv(options);
            RequestQuery.AddDateWindow(query, request.Date, request.From, request.To, request.Countback);
            var response = await GetCsvAsync(
                $"options/quotes/{Uri.EscapeDataString(symbol)}", query, cancellationToken)
                .ConfigureAwait(false);
            return (symbol, response);
        });
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.ToDictionary(item => item.symbol, item => item.response);
    }

    /// <summary>Gets the options chain for an underlying symbol as CSV.</summary>
    public async Task<CsvResponse> GetChainCsvAsync(
        OptionsChainRequest request,
        MarketDataRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateChainRequest(request);
        var query = RequestQuery.Csv(options);
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
        return await GetCsvAsync(
            $"options/chain/{Uri.EscapeDataString(request.Symbol)}", query, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<CsvResponse> GetCsvAsync(
        string path,
        IEnumerable<KeyValuePair<string, string?>> query,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetAsync(path, true, query, cancellationToken)
            .ConfigureAwait(false);
        return JsonResponseParser.CreateCsvResponse(response);
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
        RequestQuery.AddDateWindow(query, date, from, to, countback);
        var response = await _apiClient.GetAsync(
            $"options/quotes/{Uri.EscapeDataString(optionSymbol)}",
            true,
            query,
            cancellationToken).ConfigureAwait(false);
        var values = JsonResponseParser.DecodeOrDefault(
            response,
            ParseOptionQuotes,
            Array.Empty<OptionQuote>(),
            requestedColumns: options?.Columns);
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

    private static OptionStrikes ParseOptionStrikes(JsonElement root)
    {
        var strikes = new Dictionary<DateOnly, IReadOnlyList<double>>();
        foreach (var property in root.EnumerateObject())
        {
            if (!DateOnly.TryParseExact(
                    property.Name,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var expiration))
            {
                continue;
            }

            if (property.Value.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException($"Strike field '{property.Name}' must be an array.");
            }

            var values = new List<double>(property.Value.GetArrayLength());
            foreach (var value in property.Value.EnumerateArray())
            {
                if (!value.TryGetDouble(out var strike))
                {
                    throw new JsonException($"Strike field '{property.Name}' contains a non-numeric value.");
                }

                values.Add(strike);
            }

            strikes.Add(expiration, values);
        }

        return new OptionStrikes(JsonResponseParser.Timestamp(root, "updated"), strikes);
    }

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

    private static void ValidateChainRequest(OptionsChainRequest request)
    {
        if (request.Delta is < -1 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(request), request.Delta, "Delta must be between -1 and 1.");
        }

        if (request.StrikeLimit is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                request.StrikeLimit,
                "StrikeLimit must be positive.");
        }

        ValidateRange(request.MinBid, request.MaxBid, "bid", nameof(request));
        ValidateRange(request.MinAsk, request.MaxAsk, "ask", nameof(request));
        if (request.MaxBidAskSpread is < 0
            || request.MaxBidAskSpreadPct is < 0
            || request.MinOpenInterest is < 0
            || request.MinVolume is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Spread, open-interest, and volume filters cannot be negative.");
        }
    }

    private static void ValidateRange(double? minimum, double? maximum, string name, string parameterName)
    {
        if (minimum is < 0 || maximum is < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"{name} filters cannot be negative.");
        }

        if (minimum is { } min && maximum is { } max && min > max)
        {
            throw new ArgumentException($"Minimum {name} cannot exceed maximum {name}.", parameterName);
        }
    }

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
