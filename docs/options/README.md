# Options

Use `client.Options` for option lookup, expiration, strike, quote, and chain data.
Scalar overloads cover common calls; request objects are available for advanced filters.

| Operation | Simple call | Advanced request |
|---|---|---|
| Lookup | `GetLookupAsync("AAPL 250117C00150000")` | `OptionsLookupRequest` |
| Expirations | `GetExpirationsAsync("AAPL")` | `OptionsExpirationsRequest` |
| Strikes | `GetStrikesAsync("AAPL")` | `OptionsStrikesRequest` |
| One quote | `GetQuoteAsync("AAPL250117C00150000")` | `OptionsQuoteRequest` |
| Multiple quotes | `GetQuotesAsync(["AAPL250117C00150000", "AAPL250117P00150000"])` | `OptionsQuotesRequest` |
| Chain | `GetChainAsync("AAPL")` | `OptionsChainRequest` |

Every operation has a CSV counterpart. Multiple option quotes return an
`IReadOnlyDictionary<string, OptionsQuotesResponse>` and are fetched concurrently.

```csharp
var quote = await client.Options.GetQuoteAsync("AAPL250117C00150000");
var expirations = await client.Options.GetExpirationsAsync("AAPL");

// Use a request object for expiration, side, and strike filters.
var response = await client.Options.GetChainAsync(
    new OptionsChainRequest("AAPL")
    {
        Expiration = ExpirationFilter.ForDte(30),
        Side = OptionSide.Call,
        StrikeLimit = 5
    },
    cancellationToken: cancellationToken);

foreach (var quote in response.Values)
{
    Console.WriteLine($"{quote.OptionSymbol}: delta={quote.Delta}");
}
```

`ExpirationFilter` supports `ForDate`, `ForDte`, `ForRange`, `ForMonthYear`, and
`AllDates`.
`StrikeFilter` supports exact, range, and comparison filters.
