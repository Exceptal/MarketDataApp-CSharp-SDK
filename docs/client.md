# Client

`MarketDataClient` is the entry point for five API surfaces:
`Stocks`, `Options`, `Funds`, `Markets`, and `Utilities`.

## HttpClient ownership

The application injects and owns `HttpClient`. `MarketDataClient` does not dispose it.
In a console application, dispose the client you created:

```csharp
using var httpClient = new HttpClient();
var client = new MarketDataClient(httpClient);
```

In ASP.NET Core, prefer `IHttpClientFactory`:

```csharp
builder.Services.AddHttpClient("MarketData");
builder.Services.AddSingleton<MarketDataClient>(services =>
{
    var factory = services.GetRequiredService<IHttpClientFactory>();
    var configuration = services.GetRequiredService<IConfiguration>();
    var options = MarketDataClientOptions.FromConfiguration(configuration);
    return new MarketDataClient(factory.CreateClient("MarketData"), options);
});
```

A single `MarketDataClient` is safe to use concurrently. Its
`MaxConcurrentRequests` option limits in-flight requests, including internal fan-out.

## Async and cancellation

Endpoint methods are async-only. Every endpoint accepts a `CancellationToken`:

```csharp
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));

var response = await client.Stocks.GetQuoteAsync(
    new StockQuoteRequest("AAPL"),
    cancellationToken: timeout.Token);
```

Caller cancellation produces `OperationCanceledException`. The configured
`MarketDataClientOptions.Timeout` applies separately to each HTTP attempt; an SDK
timeout is surfaced as `NetworkException`.

## Client-wide rate limits

After a response, `client.LatestRateLimit` contains the latest complete snapshot, or
`null` before a response has supplied rate-limit headers:

```csharp
if (client.LatestRateLimit is { } limit)
{
    Console.WriteLine($"{limit.Remaining}/{limit.Limit} remaining");
}
```

Per-response metadata is available through `response.RateLimit`.

