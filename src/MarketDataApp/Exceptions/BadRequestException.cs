namespace MarketDataApp.Exceptions;

/// <summary>The request was malformed — invalid parameters or missing required fields.</summary>
public sealed class BadRequestException : MarketDataException
{
    /// <inheritdoc cref="MarketDataException(string, ErrorContext, Exception?)"/>
    public BadRequestException(string message, ErrorContext context) : base(message, context) { }

    /// <inheritdoc cref="MarketDataException(string, ErrorContext, Exception?)"/>
    public BadRequestException(string message, ErrorContext context, Exception inner) : base(message, context, inner) { }
}
