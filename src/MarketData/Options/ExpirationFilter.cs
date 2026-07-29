namespace MarketData.Options;

/// <summary>
/// Discriminated union for filtering options chain expirations. Construct via the nested
/// record types (<see cref="OnDate"/>, <see cref="Dte"/>, etc.) or the static factory methods.
/// </summary>
/// <remarks>
/// Pattern-match on this type to handle each variant:
/// <code>
/// filter switch
/// {
///     ExpirationFilter.OnDate { Date: var d } => ...,
///     ExpirationFilter.Dte { Days: var days } => ...,
///     ExpirationFilter.Between { From: var f, To: var t } => ...,
///     ExpirationFilter.MonthYear { Year: var y, Month: var m } => ...,
///     ExpirationFilter.All => ...,
/// }
/// </code>
/// </remarks>
public abstract record ExpirationFilter
{
    // Prevents subclassing outside this assembly.
    internal ExpirationFilter() { }

    /// <summary>Expirations on a specific calendar date.</summary>
    public sealed record OnDate(DateOnly Date) : ExpirationFilter;

    /// <summary>Expirations with approximately <see cref="Days"/> days to expiration.</summary>
    public sealed record Dte(int Days) : ExpirationFilter;

    /// <summary>Expirations falling within a date range (inclusive on both ends).</summary>
    public sealed record Between(DateOnly From, DateOnly To) : ExpirationFilter;

    /// <summary>Expirations in a specific calendar month.</summary>
    public sealed record MonthYear(int Year, int Month) : ExpirationFilter;

    /// <summary>All available expirations — no date filter applied.</summary>
    public sealed record All : ExpirationFilter;

    // ── Static factory helpers ──────────────────────────────────────────────

    /// <inheritdoc cref="OnDate"/>
    public static ExpirationFilter ForDate(DateOnly date) => new OnDate(date);

    /// <inheritdoc cref="Dte"/>
    public static ExpirationFilter ForDte(int days)
    {
        if (days < 0) throw new ArgumentOutOfRangeException(nameof(days), "Days to expiration must be non-negative.");
        return new Dte(days);
    }

    /// <inheritdoc cref="Between"/>
    public static ExpirationFilter ForRange(DateOnly from, DateOnly to)
    {
        if (from > to) throw new ArgumentException($"'{nameof(from)}' must be on or before '{nameof(to)}'.");
        return new Between(from, to);
    }

    /// <inheritdoc cref="MonthYear"/>
    public static ExpirationFilter ForMonthYear(int year, int month)
    {
        if (month is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        return new MonthYear(year, month);
    }

    /// <inheritdoc cref="All"/>
    public static ExpirationFilter AllDates() => new All();
}
