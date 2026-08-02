<div align="center">

# Market Data C#/.NET SDK
### Access Financial Data with Ease

> This is an unofficial C#/.NET SDK for [Market Data](https://www.marketdata.app/), built for **C# and Dotnet Core**. It provides developers with a powerful, easy-to-use interface to obtain real-time and historical financial data. Ideal for building financial applications, trading bots, and investment strategies.

#### Connect With The Market Data Community

[![Website](https://img.shields.io/badge/Website-marketdata.app-blue)](https://www.marketdata.app/)
[![Discord](https://img.shields.io/badge/Discord-join%20chat-7389D8.svg?logo=discord&logoColor=ffffff)](https://discord.com/invite/GmdeAVRtnT)
[![Twitter](https://img.shields.io/twitter/follow/MarketDataApp?style=social)](https://twitter.com/MarketDataApp)
[![Helpdesk](https://img.shields.io/badge/Support-Ticketing-ff69b4.svg?logo=TicketTailor&logoColor=white)](https://www.marketdata.app/dashboard/)

</div>

## Features

- **Real-time Stock Data**: Prices, quotes, candles (OHLCV), earnings, and news
- **Options Trading Data**: Options chains, expirations, quotes, and lookup
- **Mutual Funds**: Historical candles and pricing data
- **Market Status**: Real-time market open/closed status for multiple countries
- **Multiple Output Formats**: Typed objects, JSON, or CSV
- **Resilient Transport**: Retries transient failures with exponential backoff and honors `Retry-After`
- **Long Intraday Ranges**: Automatically chunks long intraday stock-candle windows and merges results
- **Built-in Retry Logic**: Automatic retry with exponential backoff for reliable data fetching
- **Rate Limit Tracking**: Per-response and client-level rate-limit snapshots
- **Type-Safe**: Records, a focused exception hierarchy, and idiomatic request objects
- **Zero Config**: Works out of the box with sensible defaults

## CSV responses

Stocks, funds, options, and market-status endpoints expose CSV methods alongside their typed
JSON methods. CSV responses preserve the normal response metadata and expose the raw content
through `Values`, `Csv`, or `RawBody`:

```csharp
var response = await client.Stocks.GetPricesCsvAsync(
    new StockPricesRequest("AAPL", "MSFT"),
    new MarketDataRequestOptions
    {
        Headers = true,
        Human = true,
        Columns = ["symbol", "mid"]
    });

File.WriteAllText("prices.csv", response.Csv);
```

## Requirements

- **dotnet 10.0 or newer**. The project is compiled with
  `dotnet build` and you can run the tests with `dotnet test`.

## API token configuration

The SDK library does not read secret stores directly. The hosting application should load
user secrets through the standard .NET configuration system and pass the resulting
configuration to the library:

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

var options = MarketDataClientOptions.FromConfiguration(builder.Configuration);
var client = new MarketDataClient(httpClient, options);
```

Initialize and store the token without writing it to source control:

```powershell
dotnet user-secrets init
dotnet user-secrets set "MarketData:ApiToken" "your-api-token"
```

The SDK library reads the token from `MarketData:ApiToken`.

## SDK design contracts

The following contracts are locked for the 1.0 design. Planned behavior that is not implemented
yet is identified explicitly.

### HTTP client and asynchronous behavior

- The application owns and injects `HttpClient`; the SDK never creates or disposes it.
- Public endpoint methods are asynchronous and accept `CancellationToken`.
- The configured `Timeout` applies independently to each HTTP attempt. Caller cancellation
  remains distinguishable from an SDK timeout.
- Endpoint requests are HTTP `GET` operations. Automatic retries never apply to parsing,
  authentication, validation, or other deterministic failures.

### Retry behavior

- `MaxRetries` means retries after the original attempt. The default of `3` therefore permits
  at most four HTTP attempts.
- Transport failures, HTTP 408, HTTP 429, and HTTP 5xx responses are eligible for retry.
- `Retry-After` takes precedence over exponential backoff when supplied by the server.
- The retry loop and backoff honor caller cancellation.
- Before 1.0, retry backoff will gain jitter and a bounded `Retry-After` delay. These changes
  will preserve the status and attempt rules above.

### Concurrency

- A single `MarketDataClient` is safe to use concurrently.
- Operations that fan out internally will share a client-wide maximum of 50 in-flight HTTP
  requests before 1.0. Callers should not rely on the current unbounded fan-out behavior.
- A fan-out operation fails if any constituent request fails; successful partial results are not
  returned as a successful response.

### Chunked response metadata

Long intraday stock-candle requests are logical requests composed of multiple HTTP requests.
The 1.0 response contract will preserve merged values and expose metadata for every constituent
request. It will not represent the final chunk's URL, request ID, raw body, or rate-limit snapshot
as metadata for the complete logical response. Until that aggregate metadata model is implemented,
callers should treat those fields on a chunked response as final-chunk metadata only.

### API contract sources

The live [OpenAPI schema](https://api.marketdata.app/schema/) is the primary source for documented
endpoint paths and wire parameters. Existing Funds and Utilities behavior is retained even though
those endpoints are currently absent from that schema.

The schema currently describes options strikes, bulk stock quotes, bulk stock candles, and a
single-symbol stock-price route that are not yet exposed by this SDK. The bulk-candles definition
also declares a required `symbol` path parameter that is absent from its path template. These
schema-only endpoints will not be implemented until their production contracts are confirmed.
