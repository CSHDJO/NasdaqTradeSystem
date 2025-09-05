using NasdaqTrader.Bot.Core;

namespace DennisMoneyBot.Utility;
internal static class PriceCalculator
{
    public static decimal GetPriceForListingOnDate(IStockListing listing, DateOnly referenceDate)
    {
        return listing.PricePoints.FirstOrDefault(p => p.Date == referenceDate)?.Price ?? 0;
    }

    public static decimal GetPricePointRatio(IStockListing listing, DateOnly today, DateOnly nextReferenceDay)
    {
        return GetPriceForListingOnDate(listing, nextReferenceDay) / GetPriceForListingOnDate(listing, today);
    }

    public static decimal GetValueOfHoldingOnDate(IHolding holding, DateOnly referenceDate)
    {
        return holding.Amount * GetPriceForListingOnDate(holding.Listing, referenceDate);
    }

    public static decimal CalculateProfitMargin(ITraderSystemContext systemContext, ITraderBot bot, IStockListing listing, DateOnly today, DateOnly nextReferenceDay)
    {
        var nextPrice = GetPriceForListingOnDate(listing, nextReferenceDay);
        var currentPrice = GetPriceForListingOnDate(listing, today);
        var priceDifference = nextPrice - currentPrice;

        var amountToBuy = 
            currentPrice == 0
            ? 0
            : Math.Min(1000, Math.Floor(systemContext.GetCurrentCash(bot) / currentPrice));
        return amountToBuy * priceDifference;
    }
}
