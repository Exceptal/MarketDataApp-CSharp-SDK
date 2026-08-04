namespace MarketDataApp.Tests.TestSupport;

internal sealed class StubHttpMessageHandler(
    Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    public IDictionary<string, string> ResponseHeaders { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;
        var response = responseFactory(request);
        foreach (var header in ResponseHeaders)
        {
            response.Headers.TryAddWithoutValidation(header.Key, new[] { header.Value });
        }

        return Task.FromResult(response);
    }
}
