using Application.Models;
using FluentAssertions;
using LondonStockService.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace LondonStockService.IntegrationTests;

public class ProgramTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task WhenValidTradePosted_ThenReturn200()
    {
        await using var api = new WebApplicationFactory<IAssemblyMarker>();
        using var client = api.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/trades",
            new PostTradesRequest(
                StockId: Application.Models.TickerSymbols.AAA,
                Price: 50,
                NoOfSharesTraded: 1,
                BrokerId: "BrokerA"));

        response.Should().HaveStatusCode(System.Net.HttpStatusCode.OK);
    }

    [Test]
    public async Task WhenTradePostedThatReducesShareCountBelowZero_ThenReturn500()
    {
        await using var api = new WebApplicationFactory<IAssemblyMarker>();
        using var client = api.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/trades",
            new PostTradesRequest(
                StockId: Application.Models.TickerSymbols.AAA,
                Price: 50,
                NoOfSharesTraded: int.MinValue,
                BrokerId: "BrokerA"));

        response.Should().HaveStatusCode(System.Net.HttpStatusCode.InternalServerError);
    }

    [Test]
    public async Task GivenManyTradesPosted_WhenGetStocksCalled_ThenReturnOnlyTheLatestPrice([Values]TickerSymbols stockId)
    {
        await using var api = new WebApplicationFactory<IAssemblyMarker>();
        using var client = api.CreateClient();

        Random random = new Random();
        int randomInt;

        await client.PostAsJsonAsync(
            "/trades",
            new PostTradesRequest(
                StockId: stockId,
                Price: randomInt = random.Next(),
                NoOfSharesTraded: 1,
                BrokerId: "BrokerA"));
        await client.PostAsJsonAsync(
            "/trades",
            new PostTradesRequest(
                StockId: stockId,
                Price: randomInt = random.Next(),
                NoOfSharesTraded: 1,
                BrokerId: "BrokerA"));
        await client.PostAsJsonAsync(
            "/trades",
            new PostTradesRequest(
                StockId: stockId,
                Price: randomInt = random.Next(),
                NoOfSharesTraded: 1,
                BrokerId: "BrokerA"));

        var response = await client.GetFromJsonAsync<GetStocksResponse>($"/stocks?stockIds={Enum.GetName(stockId)}");

        response.Should().NotBeNull();
        response!.LatestStocks.Count().Should().Be(1);
        response.LatestStocks[stockId].Price.Should().Be(randomInt);
    }

    [Test]
    public async Task WhenGetStocksCalledWithNoStockIdGiven_ThenReturnAllStocks()
    {
        await using var api = new WebApplicationFactory<IAssemblyMarker>();
        using var client = api.CreateClient();

        foreach (var stockId in Enum.GetValues<TickerSymbols>())
        {
            await client.PostAsJsonAsync(
            "/trades",
            new PostTradesRequest(
                StockId: stockId,
                Price: 1,
                NoOfSharesTraded: 1,
                BrokerId: "BrokerA"));
        }

        var response = await client.GetFromJsonAsync<GetStocksResponse>($"/stocks");

        response.Should().NotBeNull();

        foreach (var stockId in Enum.GetValues<TickerSymbols>())
        {
            response!.LatestStocks.ContainsKey(stockId).Should().BeTrue();
        }
    }
}