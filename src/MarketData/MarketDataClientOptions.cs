using Microsoft.Extensions.Configuration;

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

    /// <summary>
    /// Creates client options from application configuration.
    /// The application is responsible for loading providers such as user secrets.
    /// </summary>
    public static MarketDataClientOptions FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return new MarketDataClientOptions
        {
            ApiToken = configuration["MarketData:ApiToken"],
            BaseAddress = ReadUri(configuration["MarketData:BaseAddress"]),
            ApiVersion = configuration["MarketData:ApiVersion"] ?? "v1",
            UserAgent = configuration["MarketData:UserAgent"] ?? "marketdata-sdk-csharp/0.1.0"
        };
    }

    private static Uri ReadUri(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? new Uri("https://api.marketdata.app/")
            : new Uri(value, UriKind.Absolute);
}
