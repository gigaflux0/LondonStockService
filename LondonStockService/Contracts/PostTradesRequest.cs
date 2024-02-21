using Application.Models;
using System.ComponentModel.DataAnnotations;

namespace LondonStockService.Contracts;

public record PostTradesRequest(
    [Required] TickerSymbols StockId, 
    [Required] double Price, 
    [Required] int NoOfSharesTraded,
    [Required] string BrokerId);