using Application.Constants;
using Application.Models;
using Application.Ports;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Commands;

internal class CreateTradeHandler : IRequestHandler<CreateTrade, TradeId>
{
    private readonly IDataPersister _dataPersister;
    private readonly ILogger<CreateTradeHandler> _logger;

    public CreateTradeHandler(IDataPersister dataPersister, ILogger<CreateTradeHandler> logger)
    {
        _dataPersister = dataPersister;
        _logger = logger;
    }

    public async Task<TradeId> Handle(CreateTrade request, CancellationToken cancellationToken)
    {
        var nextTradeVersion = 1;
        var totalSharesAfterTrade = request.NoOfSharesTraded;
        if (await _dataPersister.TryGetLatestTradeByStockId([request.StockId]) is (true, var trades) && 
            trades is not null && trades.Count == 1 && trades[0].StockId == request.StockId)
        {
            Validate(request, trades[0]);
            nextTradeVersion = trades[0].Version + 1;
            totalSharesAfterTrade = trades[0].TotalSharesAfterTrade + request.NoOfSharesTraded;
        }
        else if (request.NoOfSharesTraded <= 0)
        {
            _logger.LogError(ExceptionMessages.FirstTradeCantBeZero);
            throw new ApplicationException(ExceptionMessages.FirstTradeCantBeZero);
        }

        if (await _dataPersister.TryAddTrade(request, nextTradeVersion, totalSharesAfterTrade) is (true, var trade) && trade is not null)
        {
            return trade;
        }

        _logger.LogError(ExceptionMessages.FailedToAddTrade);
        throw new ApplicationException(ExceptionMessages.FailedToAddTrade);
    }

    private void Validate(CreateTrade request, Trade latestTrade)
    {
        if (request.NoOfSharesTraded == 0)
        {
            _logger.LogError(ExceptionMessages.CantBeZero);
            throw new ApplicationException(ExceptionMessages.CantBeZero);
        }

        if (latestTrade.TotalSharesAfterTrade + request.NoOfSharesTraded < 0)
        {
            _logger.LogError(ExceptionMessages.CantBuyMoreSharesThanAvailable);
            throw new ApplicationException(ExceptionMessages.CantBuyMoreSharesThanAvailable);
        }
    }
}
