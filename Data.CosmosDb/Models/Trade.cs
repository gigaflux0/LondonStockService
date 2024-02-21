using Newtonsoft.Json;

namespace Data.CosmosDb.Models;

public record Trade(
    [property:JsonProperty("id")] string Id, 
    int Version,
    [property: JsonProperty("stockId")] string StockId, 
    double Price, 
    int NoOfSharesTraded,
    int TotalSharesAfterTrade,
    string BrokerId);
