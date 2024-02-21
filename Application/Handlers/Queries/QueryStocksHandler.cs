using Application.Constants;
using Application.Models;
using Application.Ports;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Queries;

internal class QueryStocksHandler : IRequestHandler<QueryStocks, List<Trade>>
{
    private readonly IDataPersister _dataPersister;
    private readonly ILogger<QueryStocksHandler> _logger;

    public QueryStocksHandler(IDataPersister dataPersister, ILogger<QueryStocksHandler> logger)
    {
        _dataPersister = dataPersister;
        _logger = logger;
    }

    public async Task<List<Trade>> Handle(QueryStocks request, CancellationToken cancellationToken)
    {
        if (await _dataPersister.TryGetLatestTradeByStockId(request.stocks) is (true, var trades))
        {
            return trades;
        }

        _logger.LogError(ExceptionMessages.Query);
        throw new ApplicationException(ExceptionMessages.Query);
    }
}
