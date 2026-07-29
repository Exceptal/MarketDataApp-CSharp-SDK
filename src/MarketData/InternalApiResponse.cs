namespace MarketData;

internal sealed record InternalApiResponse(
    byte[] Body,
    Uri RequestUrl,
    int StatusCode,
    string? RequestId,
    RateLimitSnapshot? RateLimit);
