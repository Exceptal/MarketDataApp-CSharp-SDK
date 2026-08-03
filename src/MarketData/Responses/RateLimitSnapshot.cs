namespace MarketData;

/// <summary>
/// Rate-limit information parsed from the <c>x-api-ratelimit-*</c> response headers.
/// Exposed on every <see cref="MarketDataResponse{T}"/> and also available client-wide
/// via <see cref="MarketDataClient.LatestRateLimit"/>.
/// </summary>
/// <param name="Limit">Total request quota for the current billing window.</param>
/// <param name="Remaining">Requests remaining before the quota resets.</param>
/// <param name="Reset">Timestamp when the quota window resets.</param>
/// <param name="Consumed">Requests consumed in the current billing window.</param>
public record RateLimitSnapshot(
    int Limit,
    int Remaining,
    DateTimeOffset Reset,
    int Consumed);
