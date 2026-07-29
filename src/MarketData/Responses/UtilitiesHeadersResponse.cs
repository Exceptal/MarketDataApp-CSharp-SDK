namespace MarketData;

/// <summary>
/// Response for <c>utilities.Headers()</c> — the HTTP request headers received by the API server,
/// keyed by lower-cased header name. Sensitive values are redacted server-side.
/// </summary>
public sealed record UtilitiesHeadersResponse : MarketDataResponse<IReadOnlyDictionary<string, string>>;
