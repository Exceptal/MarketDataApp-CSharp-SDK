using MarketDataApp.Utilities;

namespace MarketDataApp;

/// <summary>Response for <c>utilities.Status()</c> — operational status of all API services.</summary>
public sealed record UtilitiesStatusResponse : MarketDataResponse<IReadOnlyList<ServiceStatus>>;
