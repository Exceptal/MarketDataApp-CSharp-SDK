# Options

Use `client.Options` for option lookup, expiration, strike, quote, and chain data.

| Operation | Method | Request |
|---|---|---|
| Lookup | `GetLookupAsync` | `OptionsLookupRequest` |
| Expirations | `GetExpirationsAsync` | `OptionsExpirationsRequest` |
| Strikes | `GetStrikesAsync` | `OptionsStrikesRequest` |
| One quote | `GetQuoteAsync` | `OptionsQuoteRequest` |
| Multiple quotes | `GetQuotesAsync` | `OptionsQuotesRequest` |
| Chain | `GetChainAsync` | `OptionsChainRequest` |

Every operation has a CSV counterpart. Multiple option quotes return an
`IReadOnlyDictionary<string, OptionsQuotesResponse>` and are fetched concurrently.

```csharp
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
