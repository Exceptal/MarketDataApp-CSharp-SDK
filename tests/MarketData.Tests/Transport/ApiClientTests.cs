using System.Net;
using MarketData;
using MarketData.Exceptions;
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
}
