namespace MarketDataApp.Exceptions;

/// <summary>A network-level failure prevented the request from completing (timeout, DNS, TLS, etc.).</summary>
public sealed class NetworkException : MarketDataException
{
    /// <inheritdoc cref="MarketDataException(string, ErrorContext, Exception?)"/>
    public NetworkException(string message, ErrorContext context) : base(message, context) { }

    /// <inheritdoc cref="MarketDataException(string, ErrorContext, Exception?)"/>
    public NetworkException(string message, ErrorContext context, Exception inner) : base(message, context, inner) { }
}
