# Stocks

Use `client.Stocks` for asynchronous stock endpoints. Each endpoint accepts a request
object, optional `MarketDataRequestOptions`, and a `CancellationToken`.

| Operation | Method | Request |
|---|---|---|
| Single quote | `GetQuoteAsync` | `StockQuoteRequest` |
| Multi-symbol quotes | `GetQuotesAsync` | `StockQuotesRequest` |
| Bulk quotes | `GetBulkQuotesAsync` | `StockBulkQuotesRequest` |
| Multi-symbol prices | `GetPricesAsync` | `StockPricesRequest` |
| Single price | `GetPriceAsync` | `StockPriceRequest` |
| Candles | `GetCandlesAsync` | `StockCandlesRequest` |
| News | `GetNewsAsync` | `StockNewsRequest` |
| Earnings | `GetEarningsAsync` | `StockEarningsRequest` |

Each operation has a corresponding `Get*CsvAsync` method.

```csharp
var response = await client.Stocks.GetCandlesAsync(
    new StockCandlesRequest(StockResolution.Daily, "AAPL")
    {
        Countback = 30
    },
    cancellationToken: cancellationToken);

if (!response.IsNoData)
{
    foreach (var candle in response.Values)
    {
        Console.WriteLine($"{candle.Time:yyyy-MM-dd}: {candle.Close}");
    }
}
```

`StockResolution` includes `Daily`, `Weekly`, `Monthly`, `Yearly`, and factories such
as `Minutes(5)` and `Hours(1)`. Long intraday ranges are automatically chunked and
merged; inspect `IsComposite` and `Parts`. Bulk stock candles remain deferred because
the live schema has an inconsistent path definition.

