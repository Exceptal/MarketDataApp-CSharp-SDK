using MarketDataApp;

namespace MarketDataApp.Tests.TestSupport;

internal static class MarketDataTestClient
{
    public static MarketDataClient Create(
        StubHttpMessageHandler handler,
        string? token = null)
    {
        return new MarketDataClient(
            new HttpClient(handler),
            new MarketDataClientOptions { ApiToken = token, ValidateTokenOnStartup = false });
    }

    public static MarketDataClient Create(
        HttpMessageHandler handler,
        MarketDataClientOptions options) =>
        new(new HttpClient(handler), options with { ValidateTokenOnStartup = false });

    public static HttpResponseMessage JsonResponse(string body) =>
        new(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(body)
        };
}
