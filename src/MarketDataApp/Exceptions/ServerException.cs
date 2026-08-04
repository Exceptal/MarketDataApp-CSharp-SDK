namespace MarketDataApp.Exceptions;

/// <summary>
/// The server returned a 5xx error. The SDK retries 501–599; this exception is thrown
/// when all retry attempts are exhausted. <see cref="RetryAfter"/> reflects the final
/// server-supplied <c>Retry-After</c> header, if present.
/// </summary>
public sealed class ServerException : MarketDataException
{
    /// <param name="message">Human-readable description of the error.</param>
    /// <param name="context">Diagnostic context.</param>
    /// <param name="retryAfter">Server-supplied wait duration, if present.</param>
    public ServerException(string message, ErrorContext context, TimeSpan? retryAfter = null)
        : base(message, context)
    {
        RetryAfter = retryAfter;
    }

    /// <inheritdoc cref="MarketDataException(string, ErrorContext, Exception?)"/>
    public ServerException(string message, ErrorContext context, Exception inner, TimeSpan? retryAfter = null)
        : base(message, context, inner)
    {
        RetryAfter = retryAfter;
    }

    /// <summary>
    /// How long to wait before retrying, as supplied by the server's <c>Retry-After</c> header.
    /// <c>null</c> when the header was absent.
    /// </summary>
    public TimeSpan? RetryAfter { get; }
}
