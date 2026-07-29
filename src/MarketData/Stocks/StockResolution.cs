using System.Text.RegularExpressions;

namespace MarketData.Stocks;

/// <summary>
/// Candle resolution for stock endpoints. Predefined constants cover the standard daily/weekly/
/// monthly/yearly resolutions; factory methods cover arbitrary intraday and multi-bar intervals.
/// </summary>
/// <remarks>
/// Use the static constants and factory methods to construct a resolution rather than
/// <see cref="Of(string)"/> — the latter accepts any non-blank string to support forward
/// compatibility with new API resolutions.
/// </remarks>
public readonly record struct StockResolution
{
    private readonly string _value;

    private StockResolution(string value) => _value = value;

    /// <summary>Daily candles (<c>D</c>).</summary>
    public static readonly StockResolution Daily = new("D");

    /// <summary>Weekly candles (<c>W</c>).</summary>
    public static readonly StockResolution Weekly = new("W");

    /// <summary>Monthly candles (<c>M</c>).</summary>
    public static readonly StockResolution Monthly = new("M");

    /// <summary>Yearly candles (<c>Y</c>).</summary>
    public static readonly StockResolution Yearly = new("Y");

    /// <summary>Creates an n-minute resolution (e.g. <c>5</c> → <c>"5"</c>).</summary>
    public static StockResolution Minutes(int n)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "Minutes must be positive.");
        return new($"{n}");
    }

    /// <summary>Creates an n-hour resolution (e.g. <c>4</c> → <c>"4H"</c>).</summary>
    public static StockResolution Hours(int n)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "Hours must be positive.");
        return new($"{n}H");
    }

    /// <summary>Creates an n-day resolution (e.g. <c>3</c> → <c>"3D"</c>).</summary>
    public static StockResolution Days(int n)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "Days must be positive.");
        return new($"{n}D");
    }

    /// <summary>Creates an n-week resolution (e.g. <c>2</c> → <c>"2W"</c>).</summary>
    public static StockResolution Weeks(int n)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "Weeks must be positive.");
        return new($"{n}W");
    }

    /// <summary>Creates an n-month resolution (e.g. <c>3</c> → <c>"3M"</c>).</summary>
    public static StockResolution Months(int n)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "Months must be positive.");
        return new($"{n}M");
    }

    /// <summary>Creates an n-year resolution (e.g. <c>2</c> → <c>"2Y"</c>).</summary>
    public static StockResolution Years(int n)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "Years must be positive.");
        return new($"{n}Y");
    }

    /// <summary>
    /// Wraps an arbitrary resolution string for forward compatibility.
    /// Prefer the typed constants and factory methods.
    /// </summary>
    public static StockResolution Of(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Resolution value must be non-blank.", nameof(value));
        return new(value);
    }

    /// <summary>The wire-format value sent to the API (e.g. <c>"D"</c>, <c>"5"</c>, <c>"4H"</c>).</summary>
    public string WireValue => _value ?? throw new InvalidOperationException("StockResolution was default-initialized.");

    /// <summary>
    /// Whether this resolution represents an intraday interval, which triggers automatic
    /// year-sized request chunking for long date ranges.
    /// </summary>
    public bool IsIntraday =>
        Regex.IsMatch(WireValue, @"^(?:[1-9]\d*H?|H|minutely|hourly)$", RegexOptions.IgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => WireValue;
}
