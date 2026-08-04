# Funds

Use `client.Funds` for fund and ETF OHLC candle data. Use scalar overloads for common
requests or a request object when combining date-window filters.

| Operation | Simple call | Advanced request |
|---|---|---|
| Typed candles | `GetCandlesAsync(FundResolution.Daily, "VFINX")` | `FundCandlesRequest` |
| CSV candles | `GetCandlesCsvAsync(FundResolution.Daily, "VFINX")` | `FundCandlesRequest` |

```csharp
var latest = await client.Funds.GetCandlesAsync(
    FundResolution.Daily,
    "VFINX",
    countback: 20,
    cancellationToken: cancellationToken);

// Use a request object when more date-window control is needed.
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
