using System.Net;
using System.Text;
using MarketData.Funds;
using MarketData.Markets;
using MarketData.Options;
using MarketData.Stocks;
using MarketData.Tests.TestSupport;

namespace MarketData.Tests.Csv;

public sealed class CsvApiTests
{
    [Fact]
    public async Task StocksCsv_PreservesBodyMetadataAndCsvOptions()
    {
        var handler = CreateHandler("symbol,mid\r\nAAPL,150.25\r\n");
        var client = MarketDataTestClient.Create(handler);

        var response = await client.Stocks.GetPricesCsvAsync(
            new StockPricesRequest("AAPL"),
            new MarketDataRequestOptions
            {
                Headers = false,
                Human = true,
                Columns = ["symbol", "mid"]
            });

        Assert.Equal("symbol,mid\r\nAAPL,150.25\r\n", response.Values);
        Assert.Equal(response.Values, response.RawBody);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("request-1", response.RequestId);
        Assert.NotNull(response.RateLimit);
        Assert.Equal("/v1/stocks/prices/", handler.LastRequest!.RequestUri!.AbsolutePath);
        Assert.Contains("format=csv", handler.LastRequest.RequestUri.Query);
        Assert.Contains("headers=false", handler.LastRequest.RequestUri.Query);
        Assert.Contains("human=true", handler.LastRequest.RequestUri.Query);
        Assert.Contains("columns=symbol%2Cmid", handler.LastRequest.RequestUri.Query);
    }

    [Fact]
    public async Task FundsCsv_UsesFundEndpoint()
    {
        var handler = CreateHandler("t,o,h,l,c\r\n1737072000,1,2,0.5,1.5\r\n");
        var client = MarketDataTestClient.Create(handler);

        var response = await client.Funds.GetCandlesCsvAsync(
            new FundCandlesRequest(FundResolution.Daily, "SPY"));

        Assert.Contains("1737072000", response.Csv);
        Assert.Equal("/v1/funds/candles/D/SPY/", handler.LastRequest!.RequestUri!.AbsolutePath);
        Assert.Contains("format=csv", handler.LastRequest.RequestUri.Query);
    }

    [Fact]
    public async Task OptionsCsv_MapsChainFilters()
    {
        var handler = CreateHandler("optionSymbol,strike\r\nAAPL250117C00150000,150\r\n");
        var client = MarketDataTestClient.Create(handler);

        var response = await client.Options.GetChainCsvAsync(
            new OptionsChainRequest("AAPL") { Side = OptionSide.Call });

        Assert.Contains("optionSymbol", response.Values);
        Assert.Equal("/v1/options/chain/AAPL/", handler.LastRequest!.RequestUri!.AbsolutePath);
        Assert.Contains("format=csv", handler.LastRequest.RequestUri.Query);
        Assert.Contains("side=call", handler.LastRequest.RequestUri.Query);
    }

    [Fact]
    public async Task MarketsCsv_UsesStatusEndpoint()
    {
        var handler = CreateHandler("date,status\r\n2025-01-10,open\r\n");
        var client = MarketDataTestClient.Create(handler);

        var response = await client.Markets.GetStatusCsvAsync(
            new MarketStatusRequest { Country = "US" });

        Assert.Equal("date,status\r\n2025-01-10,open\r\n", response.Values);
        Assert.Equal("/v1/markets/status/", handler.LastRequest!.RequestUri!.AbsolutePath);
        Assert.Contains("country=US", handler.LastRequest.RequestUri.Query);
        Assert.Contains("format=csv", handler.LastRequest.RequestUri.Query);
    }

    private static StubHttpMessageHandler CreateHandler(string body)
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "text/csv")
        });
        handler.ResponseHeaders["x-request-id"] = "request-1";
        handler.ResponseHeaders["x-api-ratelimit-limit"] = "100";
        handler.ResponseHeaders["x-api-ratelimit-remaining"] = "99";
        handler.ResponseHeaders["x-api-ratelimit-reset"] = "1737072000";
        handler.ResponseHeaders["x-api-ratelimit-consumed"] = "1";
        return handler;
    }
}
