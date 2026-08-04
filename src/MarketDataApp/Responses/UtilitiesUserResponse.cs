using MarketDataApp.Utilities;

namespace MarketDataApp;

/// <summary>Response for <c>utilities.User()</c> — authenticated user account details and quota.</summary>
public sealed record UtilitiesUserResponse : MarketDataResponse<User>;
