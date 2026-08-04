// QuickStart — Market Data C#/.NET SDK
//
// Demonstrates: initialization, typed calls, cancellation, CSV export, and
// exception handling. No live network call is made unless MARKETDATA_TOKEN is configured.
//
// Setup (dotnet user-secrets):
//   cd examples/QuickStart
//   dotnet user-secrets init
//   dotnet user-secrets set "MARKETDATA_TOKEN" "your-api-token"
//
// Or supply the token via an environment variable:
//   MARKETDATA_TOKEN=your-api-token dotnet run
//
// Without a token the program prints the quick-start patterns but skips network calls.

using MarketData;
using MarketData.Exceptions;
using MarketData.Stocks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

// ── 1. Configuration via .NET IConfiguration (user-secrets or environment) ──────
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration
    .AddEnvironmentVariables()         // MARKETDATA_TOKEN, etc.
    .AddUserSecrets<Program>(optional: true); // dotnet user-secrets for local dev

// Build MarketDataClientOptions from configuration.
// Reads all MARKETDATA_* keys from any registered provider.
var options = MarketDataClientOptions.FromConfiguration(builder.Configuration);

// ── 2. Client lifetime ───────────────────────────────────────────────────────────
// The application owns and injects HttpClient. MarketDataClient never disposes it.
// For ASP.NET Core use IHttpClientFactory (see examples/WebApiSample).
using var httpClient = new HttpClient();
var client = new MarketDataClient(httpClient, options);

if (options.ApiToken is null)
{
    Console.WriteLine("No API token found. Set MARKETDATA_TOKEN or use dotnet user-secrets.");
    Console.WriteLine("Printing patterns only — skipping live calls.");
    PrintPatterns();
    return;
}

// ── 3. Cancellation ──────────────────────────────────────────────────────────────
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var cancellationToken = cts.Token;

// ── 4. Typed call — single-symbol quote ─────────────────────────────────────────
try
{
    Console.WriteLine("Fetching AAPL quote...");
    var quoteResponse = await client.Stocks.GetQuoteAsync(
        new StockQuoteRequest("AAPL"),
        cancellationToken: cancellationToken);

    if (quoteResponse.IsNoData)
    {
        Console.WriteLine("No data returned for AAPL.");
    }
    else
    {
        foreach (var q in quoteResponse.Values)
        {
            Console.WriteLine($"{q.Symbol}: mid={q.Mid:F2}  last={q.Last:F2}  volume={q.Volume:N0}");
        }
    }

    // Rate-limit snapshot is updated after every response.
    if (client.LatestRateLimit is { } rl)
    {
        Console.WriteLine($"Rate limit: {rl.Remaining}/{rl.Limit} remaining, resets {rl.Reset:HH:mm} UTC");
    }
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Request cancelled by the caller.");
}
catch (AuthenticationException ex)
{
    Console.Error.WriteLine($"Auth failed ({ex.StatusCode}): {ex.Message}");
    return;
}
catch (RateLimitException ex)
{
    var wait = ex.RetryAfter is { } ra ? $", retry after {ra.TotalSeconds:F0}s" : string.Empty;
    Console.Error.WriteLine($"Rate limited{wait}: {ex.Message}");
}
catch (MarketDataException ex)
{
    Console.Error.WriteLine($"{ex.ExceptionType} [{ex.StatusCode}]: {ex.Message}");
    Console.Error.WriteLine(ex.GetSupportInfo());
}

// ── 5. Typed call — OHLCV candles ────────────────────────────────────────────────
try
{
    Console.WriteLine("\nFetching MSFT daily candles (last 5)...");
    var candleResponse = await client.Stocks.GetCandlesAsync(
        new StockCandlesRequest(StockResolution.Daily, "MSFT") { Countback = 5 },
        cancellationToken: cancellationToken);

    foreach (var c in candleResponse.Values)
    {
        Console.WriteLine($"  {c.Time:yyyy-MM-dd}  O={c.Open:F2}  H={c.High:F2}  L={c.Low:F2}  C={c.Close:F2}  V={c.Volume:N0}");
    }

    Console.WriteLine($"Composite: {candleResponse.IsComposite}  Parts: {candleResponse.Parts.Count}");
}
catch (MarketDataException ex)
{
    Console.Error.WriteLine($"Candles failed: {ex.ExceptionType} — {ex.Message}");
}

// ── 6. CSV export ────────────────────────────────────────────────────────────────
try
{
    Console.WriteLine("\nFetching prices as CSV...");
    var csvResponse = await client.Stocks.GetPricesCsvAsync(
        new StockPricesRequest("AAPL", "MSFT", "TSLA"),
        new MarketDataRequestOptions { Headers = true, Human = true },
        cancellationToken);

    const string outputPath = "prices.csv";
    File.WriteAllText(outputPath, csvResponse.Csv);
    Console.WriteLine($"Saved {csvResponse.Csv.Length} characters to {outputPath}");
}
catch (MarketDataException ex)
{
    Console.Error.WriteLine($"CSV export failed: {ex.Message}");
}

// ── 7. Bulk quotes ───────────────────────────────────────────────────────────────
try
{
    Console.WriteLine("\nFetching bulk quotes for AAPL, MSFT, GOOG...");
    var bulkResponse = await client.Stocks.GetBulkQuotesAsync(
        new StockBulkQuotesRequest("AAPL", "MSFT", "GOOG"),
        cancellationToken: cancellationToken);

    foreach (var q in bulkResponse.Values)
    {
        Console.WriteLine($"  {q.Symbol}: mid={q.Mid:F2}");
    }
}
catch (MarketDataException ex)
{
    Console.Error.WriteLine($"Bulk quotes failed: {ex.Message}");
}

Console.WriteLine("\nDone.");
return;

static void PrintPatterns()
{
    Console.WriteLine("""

    ── Initialization ───────────────────────────────────────────────────────────
    using var httpClient = new HttpClient();
    var options = new MarketDataClientOptions { ApiToken = "..." };
    var client = new MarketDataClient(httpClient, options);

    ── Typed call ───────────────────────────────────────────────────────────────
    var quote = await client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL"), cancellationToken: ct);

    ── CSV export ───────────────────────────────────────────────────────────────
    var csv = await client.Stocks.GetPricesCsvAsync(new StockPricesRequest("AAPL", "MSFT"));
    File.WriteAllText("prices.csv", csv.Csv);

    ── Exception handling ───────────────────────────────────────────────────────
    catch (RateLimitException ex) { /* ex.RetryAfter */ }
    catch (AuthenticationException ex) { /* 401/403 */ }
    catch (MarketDataException ex) { Console.Error.WriteLine(ex.GetSupportInfo()); }

    """);
}
