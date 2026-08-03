# Settings and request options

## Client options

`MarketDataClientOptions.FromConfiguration(IConfiguration)` reads these application
keys:

| Key | Property | Default |
|---|---|---|
| `MarketData:ApiToken` | `ApiToken` | `null` |
| `MarketData:BaseAddress` | `BaseAddress` | `https://api.marketdata.app/` |
| `MarketData:ApiVersion` | `ApiVersion` | `v1` |
| `MarketData:Timeout` | `Timeout` | 99 seconds |
| `MarketData:MaxRetries` | `MaxRetries` | 3 retries |
| `MarketData:MaxConcurrentRequests` | `MaxConcurrentRequests` | 50 |

Advanced retry delays, jitter, `TimeProvider`, and `UserAgent` are configured
programmatically:

```csharp
var options = new MarketDataClientOptions
{
    ApiToken = token,
    Timeout = TimeSpan.FromSeconds(30),
    MaxRetries = 2,
    RetryBaseDelay = TimeSpan.FromMilliseconds(250),
    RetryMaxDelay = TimeSpan.FromSeconds(10),
    MaxRetryAfter = TimeSpan.FromMinutes(2),
    RetryJitterFactor = 0.2,
    TimeProvider = TimeProvider.System,
    UserAgent = "my-app/1.0"
};
```

## Request objects

Endpoint parameters are represented by request records. Required values are constructor
arguments; optional values use `init` properties:

```csharp
var request = new StockCandlesRequest(StockResolution.Daily, "AAPL")
{
    Countback = 30,
    AdjustDividends = true
};
```

## MarketDataRequestOptions

Pass an optional `MarketDataRequestOptions` to any endpoint:

```csharp
var response = await client.Stocks.GetQuotesAsync(
    new StockQuotesRequest("AAPL", "MSFT"),
    new MarketDataRequestOptions
    {
        DateFormat = DateFormat.Timestamp,
        Mode = Mode.Delayed,
        Limit = 50,
        Columns = ["symbol", "last"]
    },
    cancellationToken);
```

`Headers` and `Human` apply to CSV requests. Typed JSON responses use
`System.Text.Json`; endpoint responses expose typed values and response metadata rather
than a configurable global serializer.

## Response metadata and files

Every response provides `StatusCode`, `RequestUrl`, `RequestId`, `RateLimit`,
`IsNoData`, `IsComposite`, and `Parts`. The raw body can be accessed through
`RawBody` or saved:

```csharp
await response.SaveToFileAsync("quotes.json", cancellationToken);
```

CSV responses expose the same text through `Values`, `Csv`, and `RawBody`.

