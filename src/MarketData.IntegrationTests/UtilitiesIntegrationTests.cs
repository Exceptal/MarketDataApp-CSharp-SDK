namespace MarketData.IntegrationTests;

public sealed class UtilitiesIntegrationTests : IntegrationTestBase
{
    [IntegrationFact]
    public async Task StatusAndUser_ReturnExpectedShapes()
    {
        var status = await Client.Utilities.GetStatusAsync();
        var user = await Client.Utilities.GetUserAsync();

        AssertSuccess(status.StatusCode);
        Assert.NotEmpty(status.Values);
        Assert.All(status.Values, service => Assert.False(string.IsNullOrWhiteSpace(service.Service)));
        AssertSuccess(user.StatusCode);
        Assert.True(user.Values.RequestsLimit >= 0);
    }
}
