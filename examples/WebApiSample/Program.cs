// WebApiSample — Market Data C#/.NET SDK in ASP.NET Core
//
// Demonstrates:
//  - DI-managed HttpClient via IHttpClientFactory
//  - MarketDataClient registered as a singleton service
//  - Configuration loaded from appsettings.json + user secrets
//  - Minimal API endpoints that return live market data
//
// Setup:
//   cd examples/WebApiSample
//   dotnet user-secrets init
//   dotnet user-secrets set "MarketData:ApiToken" "your-api-token"
//   dotnet run

using MarketData;
using MarketData.Exceptions;
using MarketData.Options;
using MarketData.Stocks;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ────────────────────────────────────────────────────────────────
// In production, supply MarketData:ApiToken through environment variables or a
// secrets manager — never store the token in appsettings.json.
builder.Configuration.AddUserSecrets<Program>(optional: true);

// ── HttpClient via IHttpClientFactory ────────────────────────────────────────────
// Register a named client. The SDK never creates or disposes the HttpClient;
// the factory manages pooling and handler lifetime.
builder.Services.AddHttpClient("MarketData");

// ── MarketDataClient as a singleton ─────────────────────────────────────────────
// A single MarketDataClient is safe to share across concurrent requests.
builder.Services.AddSingleton<MarketDataClient>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var config = sp.GetRequiredService<IConfiguration>();

    // FromConfiguration reads all MarketData:* keys:
    //   MarketData:ApiToken, MarketData:BaseAddress, MarketData:Timeout,
    //   MarketData:MaxRetries, MarketData:RetryBaseDelay, MarketData:RetryMaxDelay,
    //   MarketData:MaxRetryAfter, MarketData:RetryJitterFactor,
    //   MarketData:MaxConcurrentRequests, MarketData:ApiVersion, MarketData:UserAgent
    var options = MarketDataClientOptions.FromConfiguration(config);
    return new MarketDataClient(factory.CreateClient("MarketData"), options);
});

var app = builder.Build();

// ── Endpoints ────────────────────────────────────────────────────────────────────

// GET /quote/{symbol}  →  latest real-time quote
app.MapGet("/quote/{symbol}", async (
    string symbol,
    MarketDataClient marketData,
    CancellationToken ct) =>
{
    try
    {
        var response = await marketData.Stocks.GetQuoteAsync(
            new StockQuoteRequest(symbol),
            cancellationToken: ct);

        return response.IsNoData
            ? Results.NotFound(new { symbol, error = "No data available." })
            : Results.Ok(response.Values);
    }
    catch (AuthenticationException)
    {
        return Results.Unauthorized();
    }
    catch (RateLimitException)
    {
        return Results.StatusCode(429);
    }
    catch (MarketDataException ex)
    {
        return Results.Problem(ex.Message, statusCode: ex.StatusCode > 0 ? ex.StatusCode : 502);
    }
});

// GET /price/{symbol}  →  single-symbol path-based price
app.MapGet("/price/{symbol}", async (
    string symbol,
    MarketDataClient marketData,
    CancellationToken ct) =>
{
    try
    {
        var response = await marketData.Stocks.GetPriceAsync(
            new StockPriceRequest(symbol),
            cancellationToken: ct);

        return response.IsNoData
            ? Results.NotFound(new { symbol })
            : Results.Ok(response.Values);
    }
    catch (MarketDataException ex)
    {
        return Results.Problem(ex.Message, statusCode: ex.StatusCode > 0 ? ex.StatusCode : 502);
    }
});

// GET /candles/{symbol}?countback=5&resolution=D  →  OHLCV candles
app.MapGet("/candles/{symbol}", async (
    string symbol,
    int countback,
    string resolution,
    MarketDataClient marketData,
    CancellationToken ct) =>
{
    try
    {
        var res = StockResolution.Of(resolution);
        var response = await marketData.Stocks.GetCandlesAsync(
            new StockCandlesRequest(res, symbol) { Countback = countback },
            cancellationToken: ct);

        return response.IsNoData
            ? Results.NotFound(new { symbol })
            : Results.Ok(new
            {
                symbol,
                resolution,
                isComposite = response.IsComposite,
                parts = response.Parts.Count,
                candles = response.Values
            });
    }
    catch (MarketDataException ex)
    {
        return Results.Problem(ex.Message, statusCode: ex.StatusCode > 0 ? ex.StatusCode : 502);
    }
});

// GET /options/chain/{symbol}  →  options chain
app.MapGet("/options/chain/{symbol}", async (
    string symbol,
    MarketDataClient marketData,
    CancellationToken ct) =>
{
    try
    {
        var response = await marketData.Options.GetChainAsync(
            new OptionsChainRequest(symbol),
            cancellationToken: ct);

        return Results.Ok(response.Values);
    }
    catch (MarketDataException ex)
    {
        return Results.Problem(ex.Message, statusCode: ex.StatusCode > 0 ? ex.StatusCode : 502);
    }
});

// GET /market/status?country=US  →  market open/closed status
app.MapGet("/market/status", async (
    string? country,
    MarketDataClient marketData,
    CancellationToken ct) =>
{
    try
    {
        var response = await marketData.Markets.GetStatusAsync(
            new MarketData.Markets.MarketStatusRequest { Country = country ?? "US" },
            cancellationToken: ct);

        return Results.Ok(response.Values);
    }
    catch (MarketDataException ex)
    {
        return Results.Problem(ex.Message, statusCode: ex.StatusCode > 0 ? ex.StatusCode : 502);
    }
});

// GET /ratelimit  →  latest rate-limit snapshot seen by this client
app.MapGet("/ratelimit", (MarketDataClient marketData) =>
    marketData.LatestRateLimit is { } rl
        ? Results.Ok(rl)
        : Results.NoContent());

app.Run();
