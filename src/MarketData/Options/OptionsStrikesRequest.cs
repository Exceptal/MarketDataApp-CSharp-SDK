namespace MarketData.Options;

/// <summary>Parameters for <c>GET /v1/options/strikes/{underlying}/</c>.</summary>
public sealed record OptionsStrikesRequest
{
    /// <summary>Underlying ticker symbol.</summary>
    public string Underlying { get; init; }

    /// <summary>Return strikes as of this historical date.</summary>
    public DateOnly? Date { get; init; }

    /// <summary>Limit strikes to this expiration date.</summary>
    public DateOnly? Expiration { get; init; }

    /// <summary>Initializes a request for <paramref name="underlying"/>.</summary>
    public OptionsStrikesRequest(string underlying)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(underlying);
        Underlying = underlying;
    }
}
