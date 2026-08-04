namespace MarketData.Options;

/// <summary>
/// Discriminated union for filtering option chain strikes. Construct via the nested record
/// types (<see cref="Exact"/>, <see cref="Range"/>, <see cref="Comparison"/>) or the static
/// factory methods.
/// </summary>
public abstract record StrikeFilter
{
    // Prevents subclassing outside this assembly.
    internal StrikeFilter() { }

    /// <summary>Exactly one strike price.</summary>
    public sealed record Exact(decimal Price) : StrikeFilter;

    /// <summary>Strikes within an inclusive price range.</summary>
    public sealed record Range(decimal Min, decimal Max) : StrikeFilter;

    /// <summary>Strikes satisfying a comparison against a threshold price.</summary>
    public sealed record Comparison(StrikeFilter.ComparisonOperator Op, decimal Price) : StrikeFilter;

    /// <summary>Comparison operators for <see cref="Comparison"/> strike filters.</summary>
    public enum ComparisonOperator
    {
        /// <summary>Greater than (&gt;).</summary>
        Gt,

        /// <summary>Greater than or equal to (&gt;=).</summary>
        Gte,

        /// <summary>Less than (&lt;).</summary>
        Lt,

        /// <summary>Less than or equal to (&lt;=).</summary>
        Lte,
    }

    // ── Static factory helpers ──────────────────────────────────────────────

    /// <inheritdoc cref="Exact"/>
    public static StrikeFilter ForExact(decimal price) => new Exact(price);

    /// <inheritdoc cref="Range"/>
    public static StrikeFilter ForRange(decimal min, decimal max)
    {
        if (min > max) throw new ArgumentException($"'{nameof(min)}' must be less than or equal to '{nameof(max)}'.");
        return new Range(min, max);
    }

    /// <inheritdoc cref="Comparison"/>
    public static StrikeFilter ForComparison(ComparisonOperator op, decimal price) => new Comparison(op, price);
}

internal static class StrikeFilterExtensions
{
    internal static string OperatorWireValue(this StrikeFilter.ComparisonOperator op) => op switch
    {
        StrikeFilter.ComparisonOperator.Gt => ">",
        StrikeFilter.ComparisonOperator.Gte => ">=",
        StrikeFilter.ComparisonOperator.Lt => "<",
        StrikeFilter.ComparisonOperator.Lte => "<=",
        _ => throw new ArgumentOutOfRangeException(nameof(op), op, null),
    };
}
