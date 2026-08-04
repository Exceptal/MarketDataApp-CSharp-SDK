namespace MarketDataApp.Exceptions;

/// <summary>The API key is missing, invalid, or does not have access to the requested resource.</summary>
public sealed class AuthenticationException : MarketDataException
{
    /// <inheritdoc cref="MarketDataException(string, ErrorContext, Exception?)"/>
    public AuthenticationException(string message, ErrorContext context) : base(message, context) { }

    /// <inheritdoc cref="MarketDataException(string, ErrorContext, Exception?)"/>
    public AuthenticationException(string message, ErrorContext context, Exception inner) : base(message, context, inner) { }
}
