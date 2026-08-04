# Settings and request options

## Client options

`MarketDataClientOptions.FromConfiguration(IConfiguration)` reads these application
keys:

| Key | Property | Default |
|---|---|---|
| `MARKETDATA_TOKEN` | `ApiToken` | `null` |
| `MARKETDATA_BASE_URL` | `BaseAddress` | `https://api.marketdata.app/` |
| `MARKETDATA_API_VERSION` | `ApiVersion` | `v1` |
| `MARKETDATA_TIMEOUT` | `Timeout` | 99 seconds |
| `MARKETDATA_MAX_RETRIES` | `MaxRetries` | 3 retries |
| `MARKETDATA_MAX_CONCURRENT_REQUESTS` | `MaxConcurrentRequests` | 50 |

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

## Simple endpoint calls

Endpoint methods support scalar parameters for common calls:

```csharp
var quote = await client.Stocks.GetQuoteAsync("AAPL");
var candles = await client.Stocks.GetCandlesAsync(
    StockResolution.Daily,
    "AAPL",
    countback: 30);
```

## Request objects

Use request records when several optional filters should be grouped or reused. Required
values are constructor arguments; optional values use `init` properties:

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
