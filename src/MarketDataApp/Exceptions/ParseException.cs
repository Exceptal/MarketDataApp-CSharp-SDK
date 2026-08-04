namespace MarketDataApp.Exceptions;

/// <summary>The response body could not be deserialized — unexpected format or a required field was absent.</summary>
public sealed class ParseException : MarketDataException
{
    /// <inheritdoc cref="MarketDataException(string, ErrorContext, Exception?)"/>
    public ParseException(string message, ErrorContext context) : base(message, context) { }

    /// <inheritdoc cref="MarketDataException(string, ErrorContext, Exception?)"/>
    public ParseException(string message, ErrorContext context, Exception inner) : base(message, context, inner) { }
}
