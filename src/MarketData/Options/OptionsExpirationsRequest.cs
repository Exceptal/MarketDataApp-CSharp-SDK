namespace MarketData.Options;

/// <summary>
/// Parameters for <c>GET /v1/options/expirations/{symbol}/</c>.
/// Returns all available expiration dates for a symbol.
/// </summary>
public record OptionsExpirationsRequest
{
    /// <summary>Underlying ticker symbol.</summary>
    public string Symbol { get; init; }

    /// <summary>Filter to expirations that have a contract at this strike price.</summary>
    public double? Strike { get; init; }

    /// <summary>Return expirations as-of this historical date rather than live.</summary>
    public DateOnly? Date { get; init; }

    /// <summary>Include non-standard contracts such as adjusted or mini options.</summary>
    public bool? NonStandard { get; init; }

    /// <summary>Initializes the request with the required <paramref name="symbol"/>.</summary>
    public OptionsExpirationsRequest(string symbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        Symbol = symbol;
    }
}
