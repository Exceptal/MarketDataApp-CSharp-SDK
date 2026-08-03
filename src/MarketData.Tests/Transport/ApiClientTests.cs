using System.Net;
using System.Net.Http.Headers;
using System.Diagnostics;
using MarketData;
using MarketData.Exceptions;
using MarketData.Stocks;
using MarketData.Options;
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
    public async Task RetryAfterHttpDate_UsesConfiguredTimeProvider()
    {
        var now = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var handler = new StubHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent("rate limited")
            };
            response.Headers.RetryAfter = new RetryConditionHeaderValue(now.AddSeconds(30));
            return response;
        });
        var client = CreateClient(handler, new MarketDataClientOptions
        {
            MaxRetries = 0,
            TimeProvider = new FixedTimeProvider(now)
        });

        var exception = await Assert.ThrowsAsync<RateLimitException>(
            () => client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL")));

        Assert.Equal(TimeSpan.FromSeconds(30), exception.RetryAfter);
        Assert.Equal(now, exception.Timestamp);
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

    [Fact]
    public async Task MissingStatus_MapsToParseException()
    {
        var handler = new StubHttpMessageHandler(_ => MarketDataTestClient.JsonResponse("""
        {
          "symbol": ["AAPL"],
          "last": [190.25]
        }
        """));
        var client = MarketDataTestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<ParseException>(
            () => client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL")));

        Assert.Contains("expected shape", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Missing response status", exception.InnerException!.Message);
    }

    [Fact]
    public async Task NoDataStatus_MapsToEmptyNoDataResponse()
    {
        var handler = new StubHttpMessageHandler(_ =>
            MarketDataTestClient.JsonResponse("""{"s":"no_data"}"""));
        var client = MarketDataTestClient.Create(handler);

        var response = await client.Stocks.GetQuoteAsync(new StockQuoteRequest("UNKNOWN"));

        Assert.True(response.IsNoData);
        Assert.Empty(response.Values);
    }

    [Fact]
    public async Task UnequalParallelArrays_MapToParseException()
    {
        var handler = new StubHttpMessageHandler(_ => MarketDataTestClient.JsonResponse("""
        {
          "s": "ok",
          "symbol": ["AAPL", "MSFT"],
          "last": [190.25]
        }
        """));
        var client = MarketDataTestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<ParseException>(
            () => client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL")));

        Assert.Contains("different lengths", exception.InnerException!.Message);
    }

    [Fact]
    public async Task MissingRequestedColumn_MapsToParseException()
    {
        var handler = new StubHttpMessageHandler(_ => MarketDataTestClient.JsonResponse("""
        {
          "s": "ok",
          "symbol": ["AAPL"]
        }
        """));
        var client = MarketDataTestClient.Create(handler);

        var exception = await Assert.ThrowsAsync<ParseException>(() => client.Stocks.GetQuoteAsync(
            new StockQuoteRequest("AAPL"),
            new MarketDataRequestOptions { Columns = ["symbol", "last"] }));

        Assert.Contains("Requested response column 'last'", exception.InnerException!.Message);
    }

    [Fact]
    public async Task RetryAfter_IsCapped()
    {
        var attempts = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            attempts++;
            if (attempts == 1)
            {
                var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent("temporary")
                };
                response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(5));
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
            MaxRetryAfter = TimeSpan.FromMilliseconds(20),
            RetryJitterFactor = 0
        });
        var stopwatch = Stopwatch.StartNew();

        await client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL"));

        Assert.Equal(2, attempts);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ExhaustedRateLimit_FailsPreflightWithoutSendingAgain()
    {
        var attempts = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            attempts++;
            return MarketDataTestClient.JsonResponse("""
            {
              "s": "ok",
              "symbol": ["AAPL"],
              "last": [190.25]
            }
            """);
        });
        handler.ResponseHeaders["x-api-ratelimit-limit"] = "100";
        handler.ResponseHeaders["x-api-ratelimit-remaining"] = "0";
        handler.ResponseHeaders["x-api-ratelimit-reset"] =
            DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString();
        handler.ResponseHeaders["x-api-ratelimit-consumed"] = "100";
        var client = MarketDataTestClient.Create(handler);

        await client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL"));
        var exception = await Assert.ThrowsAsync<RateLimitException>(
            () => client.Stocks.GetQuoteAsync(new StockQuoteRequest("MSFT")));

        Assert.Equal(1, attempts);
        Assert.Equal(0, exception.StatusCode);
    }

    [Fact]
    public async Task ClientWideConcurrency_IsBounded()
    {
        var handler = new ConcurrencyTrackingHandler();
        var client = MarketDataTestClient.Create(handler, new MarketDataClientOptions
        {
            MaxConcurrentRequests = 2,
            MaxRetries = 0
        });

        await client.Options.GetQuotesAsync(new OptionsQuotesRequest(
            "AAPL250117C00150000",
            "AAPL250117C00155000",
            "AAPL250117C00160000",
            "AAPL250117C00165000",
            "AAPL250117C00170000"));

        Assert.Equal(2, handler.MaximumConcurrency);
    }

    [Fact]
    public async Task CustomUserAgent_IsSentOnEveryRequest()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal("test-client/1.0", request.Headers.UserAgent.ToString());
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
            UserAgent = "test-client/1.0",
            MaxRetries = 0
        });

        await client.Stocks.GetQuoteAsync(new StockQuoteRequest("AAPL"));
    }

    [Theory]
    [InlineData("ftp://api.marketdata.app/")]
    [InlineData("https://user@example.com/")]
    [InlineData("https://api.marketdata.app/?token=value")]
    public void InvalidBaseAddress_IsRejected(string value)
    {
        using var handler = new StubHttpMessageHandler(_ => throw new InvalidOperationException());

        Assert.Throws<ArgumentException>(() => CreateClient(handler, new MarketDataClientOptions
        {
            BaseAddress = new Uri(value)
        }));
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

    private sealed class ConcurrencyTrackingHandler : HttpMessageHandler
    {
        private int _currentConcurrency;
        private int _maximumConcurrency;

        public int MaximumConcurrency => Volatile.Read(ref _maximumConcurrency);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var current = Interlocked.Increment(ref _currentConcurrency);
            InterlockedExtensions.Max(ref _maximumConcurrency, current);
            try
            {
                await Task.Delay(30, cancellationToken);
                return MarketDataTestClient.JsonResponse("""
                {
                  "s": "ok",
                  "optionSymbol": ["AAPL250117C00150000"],
                  "last": [12.5]
                }
                """);
            }
            finally
            {
                Interlocked.Decrement(ref _currentConcurrency);
            }

        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private static class InterlockedExtensions
    {
        public static void Max(ref int location, int value)
        {
            var current = Volatile.Read(ref location);
            while (current < value)
            {
                var observed = Interlocked.CompareExchange(ref location, value, current);
                if (observed == current)
                {
                    return;
                }

                current = observed;
            }
        }
    }
}
