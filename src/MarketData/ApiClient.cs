using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using MarketData.Exceptions;

namespace MarketData;

internal sealed class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly MarketDataClientOptions _options;
    private RateLimitSnapshot? _latestRateLimit;

    public ApiClient(HttpClient httpClient, MarketDataClientOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        if (_options.BaseAddress is null)
        {
            throw new ArgumentException("BaseAddress is required.", nameof(options));
        }
        if (_options.Timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Timeout must be positive.");
        }
        if (_options.MaxRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "MaxRetries cannot be negative.");
        }
        if (_options.RetryBaseDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "RetryBaseDelay cannot be negative.");
        }
        if (_options.RetryMaxDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "RetryMaxDelay cannot be negative.");
        }
    }

    public RateLimitSnapshot? LatestRateLimit => Volatile.Read(ref _latestRateLimit);

    public async Task<InternalApiResponse> GetAsync(
        string path,
        bool versioned,
        IEnumerable<KeyValuePair<string, string?>> query,
        CancellationToken cancellationToken)
    {
        var requestUri = BuildUri(path, versioned, query);
        var retryCount = 0;
        while (true)
        {
            try
            {
                return await SendOnceAsync(requestUri, cancellationToken).ConfigureAwait(false);
            }
            catch (MarketDataException exception) when (
                retryCount < _options.MaxRetries && IsRetryable(exception))
            {
                var delay = RetryDelay(exception, retryCount);
                retryCount++;
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task<InternalApiResponse> SendOnceAsync(Uri requestUri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.Timeout);
        var requestCancellationToken = timeoutCts.Token;
        if (!string.IsNullOrWhiteSpace(_options.ApiToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);
        }

        request.Headers.UserAgent.ParseAdd(_options.UserAgent);

        try
        {
            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                requestCancellationToken).ConfigureAwait(false);

            await using var responseContent = await response.Content.ReadAsStreamAsync(requestCancellationToken)
                .ConfigureAwait(false);
            using var memory = new MemoryStream();
            await responseContent.CopyToAsync(memory, requestCancellationToken).ConfigureAwait(false);
            var body = memory.ToArray();
            var requestId = GetHeader(response, "x-request-id") ?? GetHeader(response, "cf-ray");
            var rateLimit = ParseRateLimit(response.Headers);
            if (rateLimit is not null)
            {
                Volatile.Write(ref _latestRateLimit, rateLimit);
            }

            var result = new InternalApiResponse(body, requestUri, (int)response.StatusCode, requestId, rateLimit);
            if ((int)response.StatusCode is >= 200 and < 300 or 404)
            {
                return result;
            }

            throw CreateException(response.StatusCode, requestUri, requestId, response.Headers, body);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new NetworkException(
                "The Market Data API request timed out.",
                ErrorContext.ForNoResponse(requestUri, DateTimeOffset.UtcNow),
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new NetworkException(
                "The Market Data API request could not be sent.",
                ErrorContext.ForNoResponse(requestUri, DateTimeOffset.UtcNow),
                exception);
        }
    }

    private static bool IsRetryable(MarketDataException exception) =>
        exception is NetworkException
        || exception.StatusCode is 408 or 429 or >= 500;

    private TimeSpan RetryDelay(MarketDataException exception, int retryCount)
    {
        var retryAfter = exception switch
        {
            RateLimitException rateLimit => rateLimit.RetryAfter,
            ServerException server => server.RetryAfter,
            _ => null
        };
        if (retryAfter is { } serverDelay)
        {
            return serverDelay < TimeSpan.Zero ? TimeSpan.Zero : serverDelay;
        }

        var multiplier = 1L << Math.Min(retryCount, 30);
        var ticks = Math.Min(
            _options.RetryMaxDelay.Ticks,
            _options.RetryBaseDelay.Ticks > long.MaxValue / multiplier
                ? long.MaxValue
                : _options.RetryBaseDelay.Ticks * multiplier);
        return TimeSpan.FromTicks(ticks);
    }

    private Uri BuildUri(
        string path,
        bool versioned,
        IEnumerable<KeyValuePair<string, string?>> query)
    {
        var baseUri = _options.BaseAddress.AbsoluteUri.TrimEnd('/') + "/";
        var relativePath = versioned
            ? $"{_options.ApiVersion.Trim('/')}/{path.Trim('/')}/"
            : $"{path.Trim('/')}/";
        var builder = new UriBuilder(new Uri(new Uri(baseUri), relativePath));
        var queryString = query
            .Where(pair => pair.Value is not null)
            .Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value!)}");
        builder.Query = string.Join("&", queryString);
        return builder.Uri;
    }

    private static MarketDataException CreateException(
        HttpStatusCode statusCode,
        Uri requestUri,
        string? requestId,
        HttpResponseHeaders headers,
        byte[] body)
    {
        var context = ErrorContext.ForResponse(requestId, requestUri, (int)statusCode, DateTimeOffset.UtcNow);
        var detail = Encoding.UTF8.GetString(body);
        var message = string.IsNullOrWhiteSpace(detail)
            ? $"The Market Data API returned HTTP {(int)statusCode}."
            : $"The Market Data API returned HTTP {(int)statusCode}: {detail}";
        return statusCode switch
        {
            HttpStatusCode.BadRequest => new BadRequestException(message, context),
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AuthenticationException(message, context),
            HttpStatusCode.NotFound => new NotFoundException(message, context),
            HttpStatusCode.TooManyRequests => new RateLimitException(message, context, ParseRetryAfter(headers)),
            >= HttpStatusCode.InternalServerError => new ServerException(message, context, ParseRetryAfter(headers)),
            _ => new MarketDataExceptionAdapter(message, context)
        };
    }

    private static TimeSpan? ParseRetryAfter(HttpResponseHeaders headers)
    {
        if (headers.RetryAfter?.Delta is { } delta)
        {
            return delta;
        }

        if (headers.RetryAfter?.Date is { } date)
        {
            return date - DateTimeOffset.UtcNow;
        }

        return null;
    }

    private static RateLimitSnapshot? ParseRateLimit(HttpResponseHeaders headers)
    {
        if (!TryReadLong(headers, "x-api-ratelimit-limit", out var limit)
            || !TryReadLong(headers, "x-api-ratelimit-remaining", out var remaining)
            || !TryReadLong(headers, "x-api-ratelimit-reset", out var reset)
            || !TryReadLong(headers, "x-api-ratelimit-consumed", out var consumed))
        {
            return null;
        }

        return new RateLimitSnapshot(
            checked((int)limit),
            checked((int)remaining),
            DateTimeOffset.FromUnixTimeSeconds(reset),
            checked((int)consumed));
    }

    private static bool TryReadLong(HttpHeaders headers, string name, out long value)
    {
        return long.TryParse(GetHeader(headers, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static string? GetHeader(HttpResponseMessage response, string name) =>
        GetHeader(response.Headers, name) ?? GetHeader(response.Content.Headers, name);

    private static string? GetHeader(HttpHeaders headers, string name) =>
        headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;

    private sealed class MarketDataExceptionAdapter(string message, ErrorContext context)
        : MarketDataException(message, context);
}
