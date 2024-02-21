using Application.Models;
using Application.Ports;
using Microsoft.Azure.Cosmos;

namespace Data.CosmosDb.Adapters;

internal class DataPersister : IDataPersister
{
    private readonly CosmosClient _client;

    public DataPersister(CosmosClient client)
    {
        _client = client;
    }

    public async Task<(bool, TradeId?)> TryAddTrade(CreateTrade createTrade, int version, int totalSharesAfterTrade)
    {
        Database database = await _client.CreateDatabaseIfNotExistsAsync(
            id: "LondonStockDb",
            throughput: 400
        );

        var containerProperties = new ContainerProperties("trades", "/stockId")
        {
            UniqueKeyPolicy = new UniqueKeyPolicy
            {
                UniqueKeys =
                {
                    new UniqueKey
                    {
                        Paths = { "/Version" }
                    }
                }
            }
        };

        Container container = await database.CreateContainerIfNotExistsAsync(containerProperties);

        var tradeId = Guid.NewGuid();

        var trade = new Models.Trade(
            Id: tradeId.ToString(),
            Version: version,
            StockId: Enum.GetName(createTrade.StockId)!,
            Price: createTrade.Price,
            NoOfSharesTraded: createTrade.NoOfSharesTraded,
            BrokerId: createTrade.BrokerId,
            TotalSharesAfterTrade: totalSharesAfterTrade);


        try
        {
            var temp = await container.CreateItemAsync(trade);
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return (false, null);
            }

            throw;
        }

        return (true, new TradeId(tradeId));
    }

    public async Task<(bool, List<Application.Models.Trade>)> TryGetLatestTradeByStockId(TickerSymbols[] stockIds)
    {
        var container = _client.GetContainer("LondonStockDb", "trades");

        var trades = new List<Application.Models.Trade>();

        if (stockIds.Length == 0)
        {
            stockIds = Enum.GetValues<TickerSymbols>();
        }

        foreach (var stockId in stockIds)
        {
            var query = new QueryDefinition(query: "SELECT TOP 1 * FROM c WHERE c.stockId = @stockId ORDER BY c.Version DESC")
            .WithParameter("@stockId", Enum.GetName(stockId));

            using FeedIterator<Models.Trade> filteredFeed = container.GetItemQueryIterator<Models.Trade>(queryDefinition: query);

            while (filteredFeed.HasMoreResults)
            {
                FeedResponse<Models.Trade> response;

                try
                {
                    response = await filteredFeed.ReadNextAsync();

                    // Iterate query results
                    foreach (Models.Trade item in response)
                    {
                        trades.Add(new Application.Models.Trade(
                            Id: Guid.Parse(item.Id),
                            Version: item.Version,
                            StockId: Enum.Parse<TickerSymbols>(item.StockId),
                            Price: item.Price,
                            NoOfSharesTraded: item.NoOfSharesTraded,
                            TotalSharesAfterTrade: item.TotalSharesAfterTrade,
                            BrokerId: item.BrokerId));
                    }
                }
                catch (CosmosException e)
                {
                    if (e.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        throw;
                    }
                }
            }
        }

        return (true, trades);
    }
}
