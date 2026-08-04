using MarketDataApp.Options;

namespace MarketDataApp;

/// <summary>
/// Response for <c>options.Quote()</c> and for each per-symbol entry in the
/// <c>options.Quotes()</c> result dictionary.
/// </summary>
public sealed record OptionsQuotesResponse : MarketDataResponse<IReadOnlyList<OptionQuote>>;
