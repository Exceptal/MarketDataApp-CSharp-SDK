namespace MarketDataApp;

/// <summary>
/// Response for <c>options.Lookup()</c> — the canonical OCC option symbol resolved from
/// a user-supplied natural-language string.
/// </summary>
public sealed record OptionsLookupResponse : MarketDataResponse<string>;
