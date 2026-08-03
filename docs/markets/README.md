# Markets

Use `client.Markets` to retrieve open/closed status for an exchange country and date
window.

| Operation | Method | Request |
|---|---|---|
| Typed status | `GetStatusAsync` | `MarketStatusRequest` |
| CSV status | `GetStatusCsvAsync` | `MarketStatusRequest` |

```csharp
var response = await client.Markets.GetStatusAsync(
    new MarketStatusRequest
    {
        Country = "US",
        Countback = 5
    },
    cancellationToken: cancellationToken);

foreach (var day in response.Values)
{
    Console.WriteLine($"{day.Date:yyyy-MM-dd}: {day.Status}");
}
```

`Country` is a two-letter ISO 3166 code and defaults to `US`. Date windows use
`Date`, `From`/`To`, or `Countback` according to the request validation rules.

