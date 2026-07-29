namespace MarketData;

/// <summary>Optional parameters shared by JSON data endpoints.</summary>
public sealed record MarketDataRequestOptions
{
    /// <summary>Response date/time format.</summary>
    public DateFormat? DateFormat { get; init; }
    /// <summary>Requested data freshness mode.</summary>
    public Mode? Mode { get; init; }
    /// <summary>Maximum number of returned rows.</summary>
    public int? Limit { get; init; }
    /// <summary>Number of rows to skip.</summary>
    public int? Offset { get; init; }
    /// <summary>Requested response columns.</summary>
    public IReadOnlyList<string>? Columns { get; init; }
}
