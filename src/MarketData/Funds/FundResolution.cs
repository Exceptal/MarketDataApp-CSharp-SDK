namespace MarketData.Funds;

/// <summary>
/// Candle resolution for mutual fund endpoints. Only daily and longer resolutions are supported —
/// intraday resolutions are not available for fund candles.
/// </summary>
public readonly record struct FundResolution
{
    private readonly string _value;

    private FundResolution(string value) => _value = value;

    /// <summary>Daily candles (<c>D</c>).</summary>
    public static readonly FundResolution Daily = new("D");

    /// <summary>Weekly candles (<c>W</c>).</summary>
    public static readonly FundResolution Weekly = new("W");

    /// <summary>Monthly candles (<c>M</c>).</summary>
    public static readonly FundResolution Monthly = new("M");

    /// <summary>Yearly candles (<c>Y</c>).</summary>
    public static readonly FundResolution Yearly = new("Y");

    /// <summary>Creates an n-day resolution (e.g. <c>5</c> → <c>"5D"</c>).</summary>
    public static FundResolution Days(int n)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "Days must be positive.");
        return new($"{n}D");
    }

    /// <summary>Creates an n-week resolution (e.g. <c>2</c> → <c>"2W"</c>).</summary>
    public static FundResolution Weeks(int n)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "Weeks must be positive.");
        return new($"{n}W");
    }

    /// <summary>Creates an n-month resolution (e.g. <c>3</c> → <c>"3M"</c>).</summary>
    public static FundResolution Months(int n)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "Months must be positive.");
        return new($"{n}M");
    }

    /// <summary>Creates an n-year resolution (e.g. <c>2</c> → <c>"2Y"</c>).</summary>
    public static FundResolution Years(int n)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "Years must be positive.");
        return new($"{n}Y");
    }

    /// <summary>
    /// Wraps an arbitrary resolution string for forward compatibility.
    /// Prefer the typed constants and factory methods.
    /// </summary>
    public static FundResolution Of(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Resolution value must be non-blank.", nameof(value));
        return new(value);
    }

    /// <summary>The wire-format value sent to the API (e.g. <c>"D"</c>, <c>"3M"</c>).</summary>
    public string WireValue => _value ?? throw new InvalidOperationException("FundResolution was default-initialized.");

    /// <inheritdoc/>
    public override string ToString() => WireValue;
}
