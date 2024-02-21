namespace Application.Constants;

internal class ExceptionMessages
{
    public const string Query = "There was a problem querying the trades.";
    public const string FirstTradeCantBeZero = "The first trade for a stock can't reduce the total or be 0.";
    public const string FailedToAddTrade = "Failed to add trade to DB.";
    public const string CantBeZero = "The number of shares traded can't be 0.";
    public const string CantBuyMoreSharesThanAvailable = "Trying to buy more shares than are available.";
}
