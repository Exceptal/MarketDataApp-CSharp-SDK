using System.Net;
using System.Net.Http.Headers;
using System.Diagnostics;
using MarketData;
using MarketData.Exceptions;
using MarketData.Stocks;
using MarketData.Tests.TestSupport;

namespace MarketData.Tests.Transport;

public sealed class ApiClientTests
{
    [Fact]
    public async Task UnauthorizedResponse_MapsToAuthenticationException()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("invalid token")
        });
        var client = MarketDataTestClient.Create(handler);

        await Assert.ThrowsAsync<AuthenticationException>(
            () => client.Utilities.GetUserAsync());
    }

    [Fact]
    public async Task TransientServerFailures_RetryWithExponentialBackoff()
    {
        var attempts = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            attempts++;
            return attempts < 3
                ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent("temporary")
                }
                : MarketDataTestClient.JsonResponse("""
                {
                  "s": "ok",
                  "symbol": ["AAPL"],
                  "last": [190.25]
                }
                """);
        });
        var client = CreateClient(handler, new MarketDataClientOptions
        {
            MaxRetries = 2,
            RetryBaseDelay = TimeSpan.Zero
        });

        var response = await client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL"));

        Assert.Equal(3, attempts);
        Assert.Equal(190.25, response.Values[0].Last);
    }

    [Fact]
    public async Task RetryAfterHeader_IsUsedBeforeRetrying()
    {
        var attempts = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            attempts++;
            if (attempts == 1)
            {
                var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                {
                    Content = new StringContent("rate limited")
                };
                response.Headers.RetryAfter = new RetryConditionHeaderValue(
                    DateTimeOffset.UtcNow.AddMilliseconds(80));
                return response;
            }

            return MarketDataTestClient.JsonResponse("""
            {
              "s": "ok",
              "symbol": ["AAPL"],
              "last": [190.25]
            }
            """);
        });
        var client = CreateClient(handler, new MarketDataClientOptions
        {
            MaxRetries = 1,
            RetryBaseDelay = TimeSpan.Zero
        });
        var stopwatch = Stopwatch.StartNew();

        await client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL"));

        Assert.Equal(2, attempts);
        Assert.True(stopwatch.Elapsed >= TimeSpan.FromMilliseconds(40));
    }

    [Fact]
    public async Task ExhaustedRateLimitResponse_PreservesRetryAfterAndRateLimits()
    {
        var handler = new StubHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent("rate limited")
            };
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(2));
            return response;
        });
        handler.ResponseHeaders["x-api-ratelimit-limit"] = "100";
        handler.ResponseHeaders["x-api-ratelimit-remaining"] = "0";
        handler.ResponseHeaders["x-api-ratelimit-reset"] = "1737072000";
        handler.ResponseHeaders["x-api-ratelimit-consumed"] = "100";
        var client = CreateClient(handler, new MarketDataClientOptions { MaxRetries = 0 });

        var exception = await Assert.ThrowsAsync<RateLimitException>(
            () => client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL")));

        Assert.Equal(TimeSpan.FromSeconds(2), exception.RetryAfter);
        Assert.Equal(0, client.LatestRateLimit!.Remaining);
        Assert.Equal(100, client.LatestRateLimit.Consumed);
    }

    [Fact]
    public async Task IncompleteRateLimitHeaders_DoNotReplaceLatestCompleteSnapshot()
    {
        var attempts = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            attempts++;
            var response = MarketDataTestClient.JsonResponse("""
            {
              "s": "ok",
              "symbol": ["AAPL"],
              "last": [190.25]
            }
            """);
            if (attempts == 1)
            {
                response.Headers.TryAddWithoutValidation("x-api-ratelimit-limit", "100");
                response.Headers.TryAddWithoutValidation("x-api-ratelimit-remaining", "99");
                response.Headers.TryAddWithoutValidation("x-api-ratelimit-reset", "1737072000");
                response.Headers.TryAddWithoutValidation("x-api-ratelimit-consumed", "1");
            }

            return response;
        });
        var client = MarketDataTestClient.Create(handler);

        await client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL"));
        await client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL"));

        Assert.Equal(99, client.LatestRateLimit!.Remaining);
        Assert.Equal(1, client.LatestRateLimit.Consumed);
    }

    [Fact]
    public async Task CallerCancellation_IsNotConvertedToNetworkException()
    {
        using var cancellation = new CancellationTokenSource();
        var handler = new DelayingHandler();
        var client = CreateClient(handler, new MarketDataClientOptions
        {
            MaxRetries = 0,
            Timeout = TimeSpan.FromSeconds(5)
        });
        var request = client.Stocks.GetQuoteAsync(
            new StockQuoteRequest("AAPL"),
            cancellationToken: cancellation.Token);

        cancellation.CancelAfter(20);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request);
    }

    [Fact]
    public async Task RequestTimeout_IsConvertedToNetworkException()
    {
        var client = CreateClient(
            new DelayingHandler(),
            new MarketDataClientOptions
            {
                MaxRetries = 0,
                Timeout = TimeSpan.FromMilliseconds(20)
            });

        var exception = await Assert.ThrowsAsync<NetworkException>(
            () => client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL")));

        Assert.Equal(0, exception.StatusCode);
        Assert.Contains("timed out", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MalformedJson_MapsToParseException()
    {
        var handler = new StubHttpMessageHandler(_ => MarketDataTestClient.JsonResponse("{ invalid"));
        var client = MarketDataTestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<ParseException>(
            () => client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL")));

        Assert.Equal(200, exception.StatusCode);
        Assert.Same(handler.LastRequest!.RequestUri, exception.RequestUrl);
    }

    private static MarketDataClient CreateClient(
        HttpMessageHandler handler,
        MarketDataClientOptions options) =>
        new(new HttpClient(handler), options);

    private sealed class DelayingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
