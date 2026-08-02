using MarketData.Options;

namespace MarketData;

/// <summary>Response containing available option strikes grouped by expiration.</summary>
public sealed record OptionsStrikesResponse : MarketDataResponse<OptionStrikes>;
