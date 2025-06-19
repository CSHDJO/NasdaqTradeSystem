using NasdaqTrader.Bot.Core;

namespace HendrikBot;

public class DeJongeTrader : ITraderBot
{
    public string CompanyName => "de Jonge Investments";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        var listings = systemContext.GetListings();

        foreach (var holding in systemContext.GetHoldings(this))
        {
            systemContext.SellStock(this, holding.Listing, holding.Amount);
        }

        var tradeListing = listings
            .Where(c=>  c.PricePoints.Any(p => p.Date == systemContext.CurrentDate) &&  c.PricePoints.Any(p => p.Date == systemContext.CurrentDate.AddDays(1)))
            .MaxBy(c =>
                c.PricePoints.FirstOrDefault(p => p.Date == systemContext.CurrentDate.AddDays(1))?.Price-
                c.PricePoints.FirstOrDefault(p => p.Date == systemContext.CurrentDate)?.Price);

        if (tradeListing == null)
        {
            return;
        }

        var pricePoint = tradeListing.PricePoints.FirstOrDefault(p => p.Date == systemContext.CurrentDate);
        if (pricePoint == null)
        {
            return;
        }

        systemContext.BuyStock(this, tradeListing, Math.Min(1000, (int)(systemContext.GetCurrentCash(this) / pricePoint.Price)));
    }
}