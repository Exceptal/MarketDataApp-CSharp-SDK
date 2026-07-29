using MarketData.Markets;

namespace MarketData;

/// <summary>Response for <c>markets.Status()</c> — market open/closed status for one or more dates.</summary>
public sealed record MarketStatusResponse : MarketDataResponse<IReadOnlyList<MarketStatus>>;
