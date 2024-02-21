using MediatR;

namespace Application.Models;

public record CreateTrade(TickerSymbols StockId, double Price, int NoOfSharesTraded, string BrokerId) : IRequest<TradeId>;