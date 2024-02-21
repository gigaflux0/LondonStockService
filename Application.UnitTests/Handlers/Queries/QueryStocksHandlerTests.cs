using Application.Constants;
using Application.Handlers.Commands;
using Application.Handlers.Queries;
using Application.Models;
using Application.Ports;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Application.UnitTests.Handlers.Queries;

public class QueryStocksHandlerTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task GivenDataPersisterFailsToGetLatestTrade_WhenHandleCalled_ThenThrowApplicationException()
    {
        var stubDataPersister = Substitute.For<IDataPersister>();
        stubDataPersister
            .TryGetLatestTradeByStockId(default!)
            .ReturnsForAnyArgs((false, new List<Trade>()));
        var mockRequest = new QueryStocks([TickerSymbols.AAA]);
        var sut = new QueryStocksHandler(stubDataPersister, NullLogger<QueryStocksHandler>.Instance);

        var act = () => sut.Handle(mockRequest, CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage(ExceptionMessages.Query);
    }

    [Test]
    public async Task GivenDataPersisterGetsLatestTrade_WhenHandleCalled_ThenReturnTrade()
    {
        var fixture = new Fixture();
        var stubDataPersister = Substitute.For<IDataPersister>();
        var mockTrades = fixture.CreateMany<Trade>().ToList();
        stubDataPersister
            .TryGetLatestTradeByStockId(default!)
            .ReturnsForAnyArgs((true, mockTrades));
        var mockRequest = new QueryStocks([TickerSymbols.AAA]);
        var sut = new QueryStocksHandler(stubDataPersister, NullLogger<QueryStocksHandler>.Instance);

        var response = await sut.Handle(mockRequest, CancellationToken.None);

        response!.Should().Equal(mockTrades);
    }
}