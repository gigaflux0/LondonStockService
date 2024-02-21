using Application.Models;

namespace Application.Ports;

public interface IDataPersister
{
    Task<(bool, TradeId?)> TryAddTrade(CreateTrade createTrade, int version, int totalSharesAfterTrade);
    Task<(bool, List<Trade>)> TryGetLatestTradeByStockId(TickerSymbols[] stockIds);
}
