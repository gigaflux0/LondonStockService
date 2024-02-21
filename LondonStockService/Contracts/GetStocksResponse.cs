using Application.Models;

namespace LondonStockService.Contracts;

public record GetStocksResponse(IDictionary<TickerSymbols, Stock> LatestStocks);
