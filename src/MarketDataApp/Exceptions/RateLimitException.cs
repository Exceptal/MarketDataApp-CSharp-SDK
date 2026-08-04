namespace MarketDataApp.Exceptions;

/// <summary>
/// The account has exhausted its request quota. <see cref="RetryAfter"/> indicates how long
/// to wait before the quota resets; the SDK honors this automatically when retrying.
/// </summary>
public sealed class RateLimitException : MarketDataException
{
    /// <param name="message">Human-readable description of the error.</param>
    /// <param name="context">Diagnostic context.</param>
    /// <param name="retryAfter">Server-supplied wait duration, if present.</param>
    public RateLimitException(string message, ErrorContext context, TimeSpan? retryAfter = null)
        : base(message, context)
    {
        RetryAfter = retryAfter;
    }

    /// <inheritdoc cref="MarketDataException(string, ErrorContext, Exception?)"/>
    public RateLimitException(string message, ErrorContext context, Exception inner, TimeSpan? retryAfter = null)
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
