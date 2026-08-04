namespace MarketDataApp.IntegrationTests;

public abstract class IntegrationTestBase : IDisposable
{
    private readonly HttpClient _httpClient = new();

    protected IntegrationTestBase()
    {
        Client = new MarketDataClient(
            _httpClient,
            MarketDataClientOptions.FromConfiguration(IntegrationTestConfiguration.Instance));
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
