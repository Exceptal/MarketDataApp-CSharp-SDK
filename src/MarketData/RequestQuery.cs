using System.Globalization;

namespace MarketData;

internal static class RequestQuery
{
    public static List<KeyValuePair<string, string?>> From(MarketDataRequestOptions? options)
    {
        var query = new List<KeyValuePair<string, string?>>();
        if (options is null)
        {
            return query;
        }

        Add(query, "dateformat", options.DateFormat?.ToWireValue());
        Add(query, "mode", options.Mode?.ToWireValue());
        Add(query, "limit", options.Limit?.ToString(CultureInfo.InvariantCulture));
        Add(query, "offset", options.Offset?.ToString(CultureInfo.InvariantCulture));
        Add(query, "columns", options.Columns is { Count: > 0 } ? string.Join(",", options.Columns) : null);
        return query;
    }

    public static List<KeyValuePair<string, string?>> Csv(MarketDataRequestOptions? options)
    {
        var query = From(options);
        Add(query, "format", "csv");
        Add(query, "headers", options?.Headers?.ToString().ToLowerInvariant());
        Add(query, "human", options?.Human?.ToString().ToLowerInvariant());
        return query;
    }

    public static void Add(
        ICollection<KeyValuePair<string, string?>> query,
        string name,
        string? value)
    {
        if (value is not null)
        {
            query.Add(new KeyValuePair<string, string?>(name, value));
        }
    }

    public static string Date(DateOnly value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
