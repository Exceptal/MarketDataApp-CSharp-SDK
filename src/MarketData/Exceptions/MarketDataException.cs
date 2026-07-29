namespace MarketData.Exceptions;

/// <summary>
/// Abstract base for all Market Data SDK exceptions. The closed set of derived types is:
/// <see cref="AuthenticationException"/>, <see cref="BadRequestException"/>,
/// <see cref="NetworkException"/>, <see cref="NotFoundException"/>,
/// <see cref="ParseException"/>, <see cref="RateLimitException"/>, <see cref="ServerException"/>.
/// </summary>
public abstract class MarketDataException : Exception
{
    /// <param name="message">Human-readable description of the error.</param>
    /// <param name="context">Diagnostic context from the request/response.</param>
    /// <param name="inner">Underlying cause, if any.</param>
    protected MarketDataException(string message, ErrorContext context, Exception? inner = null)
        : base(message, inner)
    {
        Context = context;
    }

    /// <summary>Diagnostic context for this error.</summary>
    public ErrorContext Context { get; }

    /// <summary>Server-assigned request ID, or <c>null</c> if not available.</summary>
    public string? RequestId => Context.RequestId;

    /// <summary>URL that was requested. Query string is present for diagnostics.</summary>
    public Uri RequestUrl => Context.RequestUrl;

    /// <summary>HTTP status code, or 0 for network errors.</summary>
    public int StatusCode => Context.StatusCode;

    /// <summary>Timestamp when the exception was created.</summary>
    public DateTimeOffset Timestamp => Context.Timestamp;

    /// <summary>Simple class name of this exception type.</summary>
    public string ExceptionType => GetType().Name;

    /// <summary>
    /// Returns a formatted support-ticket block with all diagnostic fields,
    /// suitable for pasting into a bug report.
    /// </summary>
    public string GetSupportInfo()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        var eastern = TimeZoneInfo.ConvertTime(Timestamp, tz);
        return $"""
            Exception Type : {ExceptionType}
            Message        : {Message}
            Status Code    : {StatusCode}
            Request URL    : {RequestUrl}
            Request ID     : {RequestId ?? "(none)"}
            Timestamp (ET) : {eastern:yyyy-MM-dd HH:mm:ss zzz}
            """;
    }
}
