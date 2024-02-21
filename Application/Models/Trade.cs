namespace Application.Models;

public record Trade(Guid Id, int Version, TickerSymbols StockId, double Price, int NoOfSharesTraded, int TotalSharesAfterTrade, string BrokerId) : TradeId(Id);