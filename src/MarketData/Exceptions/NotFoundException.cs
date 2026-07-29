namespace MarketData.Exceptions;

/// <summary>
/// The requested resource was not found — the symbol does not exist, the date has no data,
/// or the endpoint path is incorrect.
/// </summary>
public sealed class NotFoundException : MarketDataException
{
    /// <inheritdoc cref="MarketDataException(string, ErrorContext, Exception?)"/>
    public NotFoundException(string message, ErrorContext context) : base(message, context) { }

    /// <inheritdoc cref="MarketDataException(string, ErrorContext, Exception?)"/>
    public NotFoundException(string message, ErrorContext context, Exception inner) : base(message, context, inner) { }
}
