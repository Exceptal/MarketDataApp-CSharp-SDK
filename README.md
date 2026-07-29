<div align="center">

# Market Data C#/dotnet
### Access Financial Data with Ease

> This is the official C#/DotNet SDK for [Market Data](https://www.marketdata.app/), built for **C# and Dotnet Core**. It provides developers with a powerful, easy-to-use interface to obtain real-time and historical financial data. Ideal for building financial applications, trading bots, and investment strategies.

#### Connect With The Market Data Community

[![Website](https://img.shields.io/badge/Website-marketdata.app-blue)](https://www.marketdata.app/)
[![Discord](https://img.shields.io/badge/Discord-join%20chat-7389D8.svg?logo=discord&logoColor=ffffff)](https://discord.com/invite/GmdeAVRtnT)
[![Twitter](https://img.shields.io/twitter/follow/MarketDataApp?style=social)](https://twitter.com/MarketDataApp)
[![Helpdesk](https://img.shields.io/badge/Support-Ticketing-ff69b4.svg?logo=TicketTailor&logoColor=white)](https://www.marketdata.app/dashboard/)

</div>

## Features

- **Real-time Stock Data**: Prices, quotes, candles (OHLCV), earnings, and news
- **Options Trading Data**: Complete options chains, expirations, strikes, quotes, and lookup
- **Mutual Funds**: Historical candles and pricing data
- **Market Status**: Real-time market open/closed status for multiple countries
- **Multiple Output Formats**: Typed objects, JSON, CSV, or HTML
- **Resilient Transport**: Retries transient failures with exponential backoff and honors `Retry-After`
- **Long Intraday Ranges**: Automatically chunks long intraday stock-candle windows and merges results
- **Built-in Retry Logic**: Automatic retry with exponential backoff for reliable data fetching
- **Rate Limit Tracking**: Per-response and client-level rate-limit snapshots
- **Type-Safe**: Records, a sealed exception hierarchy, and builder-based request objects
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
