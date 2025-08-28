using NasdaqTrader.Bot.Core;

namespace RafBot;

public class RafBot : ITraderBot
{
    public string CompanyName => "Scrooge McChicken";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        var tradesLeft = 5;
        foreach (var holding in systemContext.GetHoldings(this))
        {
            if (holding.Amount > 0)
            {
                systemContext.SellStock(this, holding.Listing, holding.Amount);
                tradesLeft--;
            }
        }

        var cash = systemContext.GetCurrentCash(this);
        var listings = systemContext.GetListings().Where(listing =>
            listing.PricePoints.Any(pricePoint => pricePoint.Date == systemContext.CurrentDate)
            && listing.PricePoints.Any(pricePoint => pricePoint.Date > systemContext.CurrentDate))
            .OrderBy(listing =>
                (listing.PricePoints.FirstOrDefault(pricePoint => pricePoint.Date == systemContext.CurrentDate)?.Price -
                listing.PricePoints.FirstOrDefault(pricePoint => pricePoint.Date > systemContext.CurrentDate)?.Price)
                    * Math.Min(1000, cash / listing.PricePoints.FirstOrDefault(pricePoint => pricePoint.Date == systemContext.CurrentDate).Price));

        foreach (var listing in listings)
        {
            if (tradesLeft == 0)
            {
                break;
            }

            int amount = Math.Min(1000, (int)(systemContext.GetCurrentCash(this) / listing.PricePoints.FirstOrDefault(pricePoint => pricePoint.Date == systemContext.CurrentDate).Price));
            if (amount > 0)
            {
                systemContext.BuyStock(this, listing, amount);
                tradesLeft--;
            }
        }
    }
}