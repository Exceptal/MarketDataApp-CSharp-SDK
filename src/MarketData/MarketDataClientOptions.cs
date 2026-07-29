namespace MarketData;

/// <summary>Configuration for <see cref="MarketDataClient"/>.</summary>
public sealed record MarketDataClientOptions
{
    /// <summary>Bearer token used for authenticated requests.</summary>
    public string? ApiToken { get; init; }
    /// <summary>API host URI.</summary>
    public Uri BaseAddress { get; init; } = new("https://api.marketdata.app/");
    /// <summary>Version path segment used by versioned endpoints.</summary>
    public string ApiVersion { get; init; } = "v1";
    /// <summary>Default request timeout.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(99);
    /// <summary>User-agent value sent by the client.</summary>
    public string UserAgent { get; init; } = "marketdata-sdk-csharp/0.1.0";
}
