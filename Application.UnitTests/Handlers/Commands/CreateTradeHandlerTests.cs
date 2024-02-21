using Application.Constants;
using Application.Handlers.Commands;
using Application.Models;
using Application.Ports;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Application.UnitTests.Handlers.Commands;

public class CreateTradeHandlerTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task GivenDataPersisterFailsToGetLatestTradeAndRequestNoOfSharesTradedIsLTE0_WhenHandleCalled_ThenThrowApplicationException()
    {
        var stubDataPersister = Substitute.For<IDataPersister>();
        stubDataPersister
            .TryGetLatestTradeByStockId(default!)
            .ReturnsForAnyArgs((false, new List<Trade>()));
        var fixture = new Fixture();
        var mockRequest = fixture.Create<CreateTrade>() with { NoOfSharesTraded = -1 };
        var sut = new CreateTradeHandler(stubDataPersister, NullLogger<CreateTradeHandler>.Instance);

        var act = () => sut.Handle(mockRequest, CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage(ExceptionMessages.FirstTradeCantBeZero);
    }

    [Test]
    public async Task GivenDataPersisterFailsToAddTrade_WhenHandleCalled_ThenThrowApplicationException()
    {
        var fixture = new Fixture();
        var stubDataPersister = Substitute.For<IDataPersister>();
        var mockTrades = fixture.CreateMany<Trade>().ToList();
        stubDataPersister
            .TryGetLatestTradeByStockId(default!)
            .ReturnsForAnyArgs((true, mockTrades));
        var mockRequest = fixture.Create<CreateTrade>();
        stubDataPersister
            .TryAddTrade(default!, default!, default!)
            .ReturnsForAnyArgs((false, null));
        var sut = new CreateTradeHandler(stubDataPersister, NullLogger<CreateTradeHandler>.Instance);

        var act = () => sut.Handle(mockRequest, CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage(ExceptionMessages.FailedToAddTrade);
    }

    [Test]
    public async Task GivenRequestNoOfSharesIs0_WhenHandleCalled_ThenThrowApplicationException()
    {
        var fixture = new Fixture();
        var stubDataPersister = Substitute.For<IDataPersister>();
        var mockRequest = fixture.Create<CreateTrade>() with { NoOfSharesTraded = 0 };
        var mockTrades = new List<Trade>() { fixture.Create<Trade>() with { StockId = mockRequest.StockId } };
        stubDataPersister
            .TryGetLatestTradeByStockId(default!)
            .ReturnsForAnyArgs((true, mockTrades));
        var sut = new CreateTradeHandler(stubDataPersister, NullLogger<CreateTradeHandler>.Instance);

        var act = () => sut.Handle(mockRequest, CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage(ExceptionMessages.CantBeZero);
    }

    [Test]
    public async Task GivenTotalSharesAfterTradeIsLT0_WhenHandleCalled_ThenThrowApplicationException()
    {
        var fixture = new Fixture();
        var stubDataPersister = Substitute.For<IDataPersister>();
        var mockRequest = fixture.Create<CreateTrade>() with { NoOfSharesTraded = int.MinValue };
        var mockTrades = new List<Trade>() { fixture.Create<Trade>() with { StockId = mockRequest.StockId } };
        stubDataPersister
            .TryGetLatestTradeByStockId(default!)
            .ReturnsForAnyArgs((true, mockTrades));
        var sut = new CreateTradeHandler(stubDataPersister, NullLogger<CreateTradeHandler>.Instance);

        var act = () => sut.Handle(mockRequest, CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage(ExceptionMessages.CantBuyMoreSharesThanAvailable);
    }

    [Test]
    public async Task GivenValidRequest_WhenHandleCalled_ThenReturnTrade()
    {
        var fixture = new Fixture();
        var stubDataPersister = Substitute.For<IDataPersister>();
        var mockTradeId = fixture.Create<TradeId>();
        var mockRequest = fixture.Create<CreateTrade>();
        var mockTrades = new List<Trade>() { fixture.Create<Trade>() with { StockId = mockRequest.StockId } };
        stubDataPersister
            .TryGetLatestTradeByStockId(default!)
            .ReturnsForAnyArgs((true, mockTrades));
        stubDataPersister
            .TryAddTrade(default!, default!, default!)
            .ReturnsForAnyArgs((true, mockTradeId));
        var sut = new CreateTradeHandler(stubDataPersister, NullLogger<CreateTradeHandler>.Instance);

        var response = await sut.Handle(mockRequest, CancellationToken.None);

        response.Should().Be(mockTradeId);
    }
}