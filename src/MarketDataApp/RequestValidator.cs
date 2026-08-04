namespace MarketDataApp;

internal static class RequestValidator
{
    public static void ValidateDateWindow(
        DateOnly? date,
        DateOnly? from,
        DateOnly? to,
        int? countback,
        string parameterName)
    {
        if (date.HasValue && (from.HasValue || to.HasValue || countback.HasValue))
        {
            throw new ArgumentException(
                "Date cannot be combined with From, To, or Countback.",
                parameterName);
        }

        if (countback.HasValue && from.HasValue)
        {
            throw new ArgumentException("Countback cannot be combined with From.", parameterName);
        }

        if (countback is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                countback,
                "Countback must be positive.");
        }

        if (from is { } fromValue && to is { } toValue && fromValue > toValue)
        {
            throw new ArgumentException("From must be on or before To.", parameterName);
        }
    }

    public static void ValidateRequestOptions(MarketDataRequestOptions? options)
    {
        if (options is null)
        {
            return;
        }

        if (options.Limit is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), options.Limit, "Limit must be positive.");
        }

        if (options.Offset is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), options.Offset, "Offset cannot be negative.");
        }

        if (options.Columns is { } columns
            && columns.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Columns cannot contain blank values.", nameof(options));
        }
    }
}
