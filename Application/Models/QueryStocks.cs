using MediatR;

namespace Application.Models;

public record QueryStocks(TickerSymbols[] stocks) : IRequest<List<Trade>>;
