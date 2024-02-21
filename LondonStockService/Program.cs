using Application.Models;
using LondonStockService.Contracts;
using LondonStockService.Mappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(LondonStockService.IAssemblyMarker).Assembly)
    .RegisterServicesFromAssembly(typeof(Application.IAssemblyMarker).Assembly);
});
builder.Services.AddCosmosDb();
builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new Asp.Versioning.ApiVersion(1);
    o.AssumeDefaultVersionWhenUnspecified = true;
}
);

var app = builder.Build();
var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new Asp.Versioning.ApiVersion(1))
    .ReportApiVersions()
    .Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.MapPost("/trades", async (IMediator mediator, [FromBody]PostTradesRequest request, CancellationToken cancellationToken) =>
{
    var createTrade = new PostTradesRequestMapper().Map(request);
    var tradeId = await mediator.Send(createTrade);
    var postTradesResponse = new PostTradesResponse(TradeId: tradeId.Id.ToString());
    return postTradesResponse;
})
.WithOpenApi()
.WithApiVersionSet(versionSet)
.MapToApiVersion(new Asp.Versioning.ApiVersion(1));

app.MapGet("/stocks", async (IMediator mediator, [FromQuery]TickerSymbols[] stockIds, CancellationToken cancellationToken) =>
{
    var queryStocks = new QueryStocks(stockIds);
    var trades = await mediator.Send(queryStocks);
    var stocks = new Dictionary<TickerSymbols, Stock>();
    foreach (var trade in trades)
    {
        stocks.Add(trade.StockId, new Stock(trade.Price));
    }
    return new GetStocksResponse(stocks);
})
.WithOpenApi()
.WithApiVersionSet(versionSet)
.MapToApiVersion(new Asp.Versioning.ApiVersion(1));

app.Run();
