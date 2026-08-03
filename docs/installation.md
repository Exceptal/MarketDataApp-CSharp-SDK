# Installation

## Requirements

- .NET 10.0 or newer
- C# latest stable language version

The SDK uses the platform `HttpClient` abstraction and `System.Text.Json`. Applications
provide the `HttpClient`; the SDK does not create or dispose one.

## NuGet

```powershell
dotnet add package MarketData
```

Or add the package reference to a project file:

```xml
<PackageReference Include="MarketData" Version="1.0.0" />
```

Check the [NuGet package](https://www.nuget.org/packages/MarketData) for the current
published version.

## First request

```csharp
using MarketData;
using MarketData.Stocks;

using var httpClient = new HttpClient();
var client = new MarketDataClient(
    httpClient,
    new MarketDataClientOptions { ApiToken = "your-api-token" });

var response = await client.Stocks.GetQuoteAsync(
    new StockQuoteRequest("AAPL"),
    cancellationToken: CancellationToken.None);

foreach (var quote in response.Values)
{
    Console.WriteLine($"{quote.Symbol}: {quote.Last}");
}
```

Do not replace `await` with `.Result` or `.Wait()`. See [client lifetime and DI](client.md)
for ASP.NET Core registration.

