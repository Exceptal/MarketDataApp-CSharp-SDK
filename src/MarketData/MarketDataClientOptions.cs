using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Reflection;

namespace MarketData;

/// <summary>Configuration for <see cref="MarketDataClient"/>.</summary>
public sealed record MarketDataClientOptions
{
    private const string ConfigurationPrefix = "MarketData:";
    private static readonly string DefaultUserAgentValue = CreateDefaultUserAgent();

    /// <summary>Bearer token used for authenticated requests.</summary>
    public string? ApiToken { get; init; }
    /// <summary>API host URI.</summary>
    public Uri BaseAddress { get; init; } = new("https://api.marketdata.app/");
    /// <summary>Version path segment used by versioned endpoints.</summary>
    public string ApiVersion { get; init; } = "v1";
    /// <summary>Default request timeout.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(99);
    /// <summary>Maximum number of retries after a transient request failure.</summary>
    public int MaxRetries { get; init; } = 3;
    /// <summary>Initial exponential-backoff delay between retries.</summary>
    public TimeSpan RetryBaseDelay { get; init; } = TimeSpan.FromMilliseconds(250);
    /// <summary>Maximum exponential-backoff delay when the server does not provide Retry-After.</summary>
    public TimeSpan RetryMaxDelay { get; init; } = TimeSpan.FromSeconds(30);
    /// <summary>Maximum server-provided Retry-After delay honored by automatic retries.</summary>
    public TimeSpan MaxRetryAfter { get; init; } = TimeSpan.FromMinutes(10);
    /// <summary>Fractional random jitter applied to exponential backoff, from 0 through 1.</summary>
    public double RetryJitterFactor { get; init; } = 0.2;
    /// <summary>Maximum number of HTTP requests simultaneously dispatched by this client.</summary>
    public int MaxConcurrentRequests { get; init; } = 50;
    /// <summary>Time source used by timeout, retry, and rate-limit behavior.</summary>
    public TimeProvider TimeProvider { get; init; } = TimeProvider.System;
    /// <summary>User-agent value sent by the client.</summary>
    public string UserAgent { get; init; } = DefaultUserAgentValue;

    /// <summary>
    /// Creates client options from application configuration.
    /// The application is responsible for loading providers such as user secrets.
    /// </summary>
    public static MarketDataClientOptions FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return new MarketDataClientOptions
        {
            ApiToken = configuration[$"{ConfigurationPrefix}ApiToken"],
            BaseAddress = ReadUri(configuration[$"{ConfigurationPrefix}BaseAddress"]),
            ApiVersion = configuration[$"{ConfigurationPrefix}ApiVersion"] ?? "v1",
            Timeout = ReadTimeSpan(configuration, "Timeout", TimeSpan.FromSeconds(99)),
            MaxRetries = ReadInt(configuration, "MaxRetries", 3),
            RetryBaseDelay = ReadTimeSpan(configuration, "RetryBaseDelay", TimeSpan.FromMilliseconds(250)),
            RetryMaxDelay = ReadTimeSpan(configuration, "RetryMaxDelay", TimeSpan.FromSeconds(30)),
            MaxRetryAfter = ReadTimeSpan(configuration, "MaxRetryAfter", TimeSpan.FromMinutes(10)),
            RetryJitterFactor = ReadDouble(configuration, "RetryJitterFactor", 0.2),
            MaxConcurrentRequests = ReadInt(configuration, "MaxConcurrentRequests", 50),
            UserAgent = configuration[$"{ConfigurationPrefix}UserAgent"] ?? DefaultUserAgentValue
        };
    }

    private static Uri ReadUri(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? new Uri("https://api.marketdata.app/")
            : new Uri(value, UriKind.Absolute);

    private static int ReadInt(IConfiguration configuration, string name, int defaultValue)
    {
        var configured = configuration[$"{ConfigurationPrefix}{name}"];
        if (configured is null)
        {
            return defaultValue;
        }

        return int.TryParse(configured, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new FormatException(
                $"Configuration value '{ConfigurationPrefix}{name}' must be an integer.");
    }

    private static double ReadDouble(IConfiguration configuration, string name, double defaultValue)
    {
        var configured = configuration[$"{ConfigurationPrefix}{name}"];
        if (configured is null)
        {
            return defaultValue;
        }

        return double.TryParse(configured, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new FormatException(
                $"Configuration value '{ConfigurationPrefix}{name}' must be a number.");
    }

    private static TimeSpan ReadTimeSpan(
        IConfiguration configuration,
        string name,
        TimeSpan defaultValue) =>
        ReadTimeSpanValue(configuration[$"{ConfigurationPrefix}{name}"], name, defaultValue);

    private static TimeSpan ReadTimeSpanValue(string? configured, string name, TimeSpan defaultValue)
    {
        if (configured is null)
        {
            return defaultValue;
        }

        return TimeSpan.TryParse(configured, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new FormatException(
                $"Configuration value '{ConfigurationPrefix}{name}' must be a TimeSpan.");
    }

    private static string CreateDefaultUserAgent()
    {
        var assembly = typeof(MarketDataClientOptions).Assembly;
        var version = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            .Split('+', 2)[0]
            ?? assembly.GetName().Version?.ToString(3)
            ?? "unknown";
        return $"marketdata-sdk-csharp/{version}";
    }
}
