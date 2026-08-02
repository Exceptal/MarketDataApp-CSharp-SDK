namespace MarketData.IntegrationTests;

public abstract class IntegrationTestBase : IDisposable
{
    private readonly HttpClient _httpClient = new();

    protected IntegrationTestBase()
    {
        Client = new MarketDataClient(
            _httpClient,
            new MarketDataClientOptions
            {
                ApiToken = Environment.GetEnvironmentVariable("MARKETDATA_TOKEN"),
                MaxRetries = 1
            });
    }

    protected MarketDataClient Client { get; }

    protected static void AssertSuccess(int statusCode) =>
        Assert.Contains(statusCode, new[] { 200, 203 });

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
