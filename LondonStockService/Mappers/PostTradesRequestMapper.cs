using Application.Models;
using LondonStockService.Contracts;
using Riok.Mapperly.Abstractions;

namespace LondonStockService.Mappers;

[Mapper]
public partial class PostTradesRequestMapper
{
    public partial CreateTrade Map(PostTradesRequest request);
}
