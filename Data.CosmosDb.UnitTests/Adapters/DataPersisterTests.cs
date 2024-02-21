using Application.Models;
using AutoFixture;
using Data.CosmosDb.Adapters;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Data.CosmosDb.UnitTests.Adapters;

public class DataPersisterTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task GivenValidTrade_WhenTryAddTradeCalled_ThenReturnTrueAndNotNull()
    {
        var fixture = new Fixture();
        var mockTradeId = fixture.Create<TradeId>();

        var stubCosmosClient = Substitute.For<CosmosClient>();
        var stubDatabase = Substitute.For<DatabaseResponse>();
        var stubContainer = Substitute.For<ContainerResponse>();
        var stubItemResponse = Substitute.For<ItemResponse<Trade>>();
        stubCosmosClient
            .CreateDatabaseIfNotExistsAsync((string)default!, (int)default!)
            .ReturnsForAnyArgs(stubDatabase);
        ((Database)stubDatabase)
            .CreateContainerIfNotExistsAsync(default!)
            .ReturnsForAnyArgs(stubContainer);
        ((Container)stubContainer)
            .CreateItemAsync((Trade)default!)
            .ReturnsForAnyArgs(stubItemResponse);
        
        var mockCreateTrade = fixture.Create<CreateTrade>();
        var mockVersion = fixture.Create<int>();
        var mockTotalSharesAfterTrade = fixture.Create<int>();
        

        var sut = new DataPersister(stubCosmosClient);
        var request = await sut.TryAddTrade(mockCreateTrade, mockVersion, mockTotalSharesAfterTrade);

        request.Item1.Should().BeTrue();
        request.Item2.Should().NotBeNull();
    }

    [Test]
    public async Task GivenDbConcurrencyConflict_WhenTryAddTradeCalled_ThenReturnFalseAndNull()
    {
        var fixture = new Fixture();
        var mockCosmosException = new CosmosException(
            fixture.Create<string>(), 
            System.Net.HttpStatusCode.Conflict, 
            fixture.Create<int>(), 
            fixture.Create<string>(), 
            fixture.Create<double>());

        var stubCosmosClient = Substitute.For<CosmosClient>();
        var stubDatabase = Substitute.For<DatabaseResponse>();
        var stubContainer = Substitute.For<ContainerResponse>();
        stubCosmosClient
            .CreateDatabaseIfNotExistsAsync((string)default!, (int)default!)
            .ReturnsForAnyArgs(stubDatabase);
        ((Database)stubDatabase)
            .CreateContainerIfNotExistsAsync(default)
            .ReturnsForAnyArgs(stubContainer);
        ((Container)stubContainer)
            .CreateItemAsync<Models.Trade>(default!)
            .ThrowsAsyncForAnyArgs(mockCosmosException);

        var mockCreateTrade = fixture.Create<CreateTrade>();
        var mockVersion = fixture.Create<int>();
        var mockTotalSharesAfterTrade = fixture.Create<int>();


        var sut = new DataPersister(stubCosmosClient);
        var request = await sut.TryAddTrade(mockCreateTrade, mockVersion, mockTotalSharesAfterTrade);

        request.Item1.Should().BeFalse();
        request.Item2.Should().BeNull();
    }

    [Test]
    public async Task GivenDbExceptionExceptConflict_WhenTryAddTradeCalled_ThenThrowCosmosException([Values]System.Net.HttpStatusCode statusCode)
    {
        if (statusCode == System.Net.HttpStatusCode.Conflict)
        {
            return;
        }

        var fixture = new Fixture();
        var mockCosmosException = new CosmosException(
            fixture.Create<string>(),
            statusCode,
            fixture.Create<int>(),
            fixture.Create<string>(),
            fixture.Create<double>());

        var stubCosmosClient = Substitute.For<CosmosClient>();
        var stubDatabase = Substitute.For<DatabaseResponse>();
        var stubContainer = Substitute.For<ContainerResponse>();
        stubCosmosClient
            .CreateDatabaseIfNotExistsAsync((string)default!, (int)default!)
            .ReturnsForAnyArgs(stubDatabase);
        ((Database)stubDatabase)
            .CreateContainerIfNotExistsAsync(default)
            .ReturnsForAnyArgs(stubContainer);
        ((Container)stubContainer)
            .CreateItemAsync<Models.Trade>(default!)
            .ThrowsAsyncForAnyArgs(mockCosmosException);

        var mockCreateTrade = fixture.Create<CreateTrade>();
        var mockVersion = fixture.Create<int>();
        var mockTotalSharesAfterTrade = fixture.Create<int>();


        var sut = new DataPersister(stubCosmosClient);
        var act = () => sut.TryAddTrade(mockCreateTrade, mockVersion, mockTotalSharesAfterTrade);

        await act.Should().ThrowAsync<CosmosException>();
    }

    [Test]
    public async Task GivenDbExceptionExceptNotFound_WhenTryGetLatestTradeByStockIdCalled_ThenThrowCosmosException([Values] System.Net.HttpStatusCode statusCode)
    {
        if (statusCode == System.Net.HttpStatusCode.NotFound)
        {
            return;
        }

        var fixture = new Fixture();
        var mockStockIds = fixture.CreateMany<TickerSymbols>().ToArray();

        var mockCosmosException = new CosmosException(
            fixture.Create<string>(),
            statusCode,
            fixture.Create<int>(),
            fixture.Create<string>(),
            fixture.Create<double>());

        var stubCosmosClient = Substitute.For<CosmosClient>();
        var stubContainer = Substitute.For<Container>();
        var stubFeedIterator = Substitute.For<FeedIterator<Models.Trade>>();
        var stubFeedResponse = Substitute.For<FeedResponse<Models.Trade>>();
        stubCosmosClient
            .GetContainer(default!, default!)
            .ReturnsForAnyArgs(stubContainer);
        stubContainer
            .GetItemQueryIterator<Models.Trade>((QueryDefinition)default!)
            .ReturnsForAnyArgs(stubFeedIterator);
        stubFeedIterator
            .HasMoreResults
            .Returns(true);
        stubFeedIterator
            .ReadNextAsync()
            .ThrowsAsyncForAnyArgs(mockCosmosException);

        var sut = new DataPersister(stubCosmosClient);
        var act = () => sut.TryGetLatestTradeByStockId(mockStockIds);

        await act.Should().ThrowAsync<CosmosException>();
    }

    [Test]
    public async Task GivenValidCall_WhenTryGetLatestTradeByStockIdCalled_ThenReturnTrueAndNotNull()
    {
        var fixture = new Fixture();
        var mockStockIds = fixture.CreateMany<TickerSymbols>().ToArray();

        var stubCosmosClient = Substitute.For<CosmosClient>();
        var stubContainer = Substitute.For<Container>();
        var stubFeedIterator = Substitute.For<FeedIterator<Models.Trade>>();
        var stubFeedResponse = Substitute.For<FeedResponse<Models.Trade>>();
        stubCosmosClient
            .GetContainer(default!, default!)
            .ReturnsForAnyArgs(stubContainer);
        stubContainer
            .GetItemQueryIterator<Models.Trade>((QueryDefinition)default!)
            .ReturnsForAnyArgs(stubFeedIterator);
        stubFeedIterator
            .HasMoreResults
            .Returns(false);
        stubFeedIterator
            .ReadNextAsync()
            .Returns(stubFeedResponse);

        var sut = new DataPersister(stubCosmosClient);
        var response = await sut.TryGetLatestTradeByStockId(mockStockIds);

        response.Item1.Should().BeTrue();
        response.Item2.Should().NotBeNull();
    }
}