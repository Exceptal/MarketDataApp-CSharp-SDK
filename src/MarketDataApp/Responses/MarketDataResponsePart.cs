namespace MarketDataApp;

/// <summary>Metadata and raw content for one HTTP response that contributed to a logical SDK response.</summary>
public sealed record MarketDataResponsePart
{
    /// <summary>HTTP status code.</summary>
    public required int StatusCode { get; init; }

    /// <summary>URL sent for this constituent request.</summary>
    public required Uri RequestUrl { get; init; }

    /// <summary>Server-assigned request identifier, if present.</summary>
    public string? RequestId { get; init; }

    /// <summary>Rate-limit snapshot returned with this response, if complete.</summary>
    public RateLimitSnapshot? RateLimit { get; init; }

    /// <summary>Raw response body decoded as UTF-8.</summary>
    public required string RawBody { get; init; }
}
