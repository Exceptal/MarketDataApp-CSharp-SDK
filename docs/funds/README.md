# Funds

Use `client.Funds` for fund and ETF OHLC candle data.

| Operation | Method | Request |
|---|---|---|
| Typed candles | `GetCandlesAsync` | `FundCandlesRequest` |
| CSV candles | `GetCandlesCsvAsync` | `FundCandlesRequest` |

```csharp
var response = await client.Funds.GetCandlesAsync(
    new FundCandlesRequest(FundResolution.Daily, "VFINX")
    {
        Countback = 20
    },
    cancellationToken: cancellationToken);

if (response.IsNoData)
{
    Console.WriteLine("No fund data was returned.");
}
else
{
    foreach (var candle in response.Values)
    {
        Console.WriteLine($"{candle.Time:yyyy-MM-dd}: {candle.Close}");
    }
}
```

`FundCandle` contains time and OHLC values; it does not contain stock volume.
Funds are implemented by the SDK but are currently not listed in the live schema.

