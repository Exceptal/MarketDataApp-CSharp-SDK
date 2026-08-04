using MarketDataApp.Exceptions;

namespace MarketDataApp.Tests.Exceptions;

public sealed class MarketDataExceptionTests
{
    [Fact]
    public void GetSupportInfo_ResolvesEasternTimeZone_OnAnyPlatform()
    {
        // Regression test: GetSupportInfo() must not depend on a Windows-only time zone ID
        // ("Eastern Standard Time"), which throws TimeZoneNotFoundException on Linux/macOS.
        var context = ErrorContext.ForResponse(
            requestId: "req-1",
            requestUrl: new Uri("https://api.marketdata.app/v1/stocks/quotes/AAPL/"),
            statusCode: 400,
            timestamp: new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero));
        var exception = new BadRequestException("bad request", context);

        var info = exception.GetSupportInfo();

        Assert.Contains("Exception Type : BadRequestException", info);
        Assert.Contains("Status Code    : 400", info);
        Assert.Contains("Request ID     : req-1", info);
        Assert.Contains("Timestamp (ET) :", info);
    }
}
