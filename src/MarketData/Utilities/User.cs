namespace MarketData.Utilities;

/// <summary>
/// Authenticated user account details, including quota usage.
/// All fields are non-null — the endpoint always returns the full shape.
/// </summary>
/// <param name="RequestsRemaining">API requests remaining in the current billing period.</param>
/// <param name="RequestsLimit">Total API request quota for the billing period.</param>
/// <param name="OptionsDataPermissions">
/// Describes the options data entitlement. An empty string indicates real-time access;
/// a non-empty value describes the delay (e.g. <c>"OPRA data delayed 15 minutes"</c>).
/// </param>
public record User(
    int RequestsRemaining,
    int RequestsLimit,
    string OptionsDataPermissions);
