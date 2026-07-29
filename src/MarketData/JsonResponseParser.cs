using System.Globalization;
using System.Text.Json;
using MarketData.Exceptions;

namespace MarketData;

internal static class JsonResponseParser
{
    public static JsonElement Parse(InternalApiResponse response)
    {
        try
        {
            using var document = JsonDocument.Parse(response.Body);
            return document.RootElement.Clone();
        }
        catch (JsonException exception)
        {
            throw new ParseException(
                "The Market Data API returned invalid JSON.",
                ErrorContext.ForResponse(response.RequestId, response.RequestUrl, response.StatusCode, DateTimeOffset.UtcNow),
                exception);
        }
    }

    public static IReadOnlyList<T> ReadParallelArray<T>(
        JsonElement root,
        Func<ParallelArrayRow, T> factory,
        params string[] fields)
    {
        var count = fields
            .Select(field => root.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.Array
                ? value.GetArrayLength()
                : 0)
            .DefaultIfEmpty()
            .Max();

        var rows = new List<T>(count);
        for (var index = 0; index < count; index++)
        {
            rows.Add(factory(new ParallelArrayRow(root, index)));
        }

        return rows;
    }

    public readonly struct ParallelArrayRow(JsonElement root, int index)
    {
        public string? String(string name) => Value(name) is { ValueKind: JsonValueKind.String } value
            ? value.GetString()
            : null;

        public double? Double(string name) => Value(name) is { } value && value.ValueKind == JsonValueKind.Number
            && value.TryGetDouble(out var result) ? result : null;

        public long? Long(string name) => Value(name) is { } value && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt64(out var result) ? result : null;

        public bool? Boolean(string name) => Value(name) is { } value && value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;

        public DateTimeOffset? Timestamp(string name) => ToDateTime(Value(name));

        private JsonElement? Value(string name)
        {
            if (!root.TryGetProperty(name, out var values)
                || values.ValueKind != JsonValueKind.Array
                || index >= values.GetArrayLength())
            {
                return null;
            }

            var value = values[index];
            return value.ValueKind == JsonValueKind.Null ? null : value;
        }
    }

    public static DateTimeOffset? ToDateTime(JsonElement? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value.Value.ValueKind == JsonValueKind.Number
            && value.Value.TryGetDouble(out var number))
        {
            return DateTimeOffset.UnixEpoch.AddSeconds(number);
        }

        if (value.Value.ValueKind == JsonValueKind.String
            && DateTimeOffset.TryParse(
                value.Value.GetString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var timestamp))
        {
            return timestamp;
        }

        return null;
    }

    public static DateTimeOffset? Timestamp(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) ? ToDateTime(value) : null;

    public static T Decode<T>(InternalApiResponse response, Func<JsonElement, T> decoder)
    {
        try
        {
            return decoder(Parse(response));
        }
        catch (JsonException exception)
        {
            throw new ParseException(
                "The Market Data API response did not match the expected shape.",
                ErrorContext.ForResponse(response.RequestId, response.RequestUrl, response.StatusCode, DateTimeOffset.UtcNow),
                exception);
        }
    }

    public static TResponse CreateResponse<TResponse, T>(
        InternalApiResponse response,
        T values,
        Func<TResponse, TResponse>? customize = null)
        where TResponse : MarketDataResponse<T>, new()
    {
        var result = new TResponse
        {
            Values = values,
            StatusCode = response.StatusCode,
            RequestUrl = response.RequestUrl,
            RequestId = response.RequestId,
            RateLimit = response.RateLimit,
            RawBodyBytes = response.Body
        };
        return customize is null ? result : customize(result);
    }
}
