using MarketData.Options;

namespace MarketData;

/// <summary>Response for <c>options.Chain()</c> — the full options chain for an underlying symbol.</summary>
public sealed record OptionsChainResponse : MarketDataResponse<IReadOnlyList<OptionQuote>>;
