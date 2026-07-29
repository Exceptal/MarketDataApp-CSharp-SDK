namespace MarketData.Options;

/// <summary>
/// Parameters for <c>GET /v1/options/lookup/{userInput}/</c>.
/// Resolves a user-supplied string (e.g. <c>"AAPL 150 Call 2025-01-17"</c>) to a
/// canonical OCC option symbol.
/// </summary>
public record OptionsLookupRequest
{
    /// <summary>The user-supplied input string to resolve.</summary>
    public string UserInput { get; }

    /// <summary>Initializes the request, validating that <paramref name="userInput"/> is non-empty.</summary>
    public OptionsLookupRequest(string userInput)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userInput);
        UserInput = userInput;
    }
}
