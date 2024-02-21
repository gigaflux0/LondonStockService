using System.Text.Json.Serialization;

namespace Application.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TickerSymbols
{
    AAA,
    BBB,
    CCC
}
