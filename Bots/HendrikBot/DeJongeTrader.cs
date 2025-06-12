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
            .MaxBy(c =>
                c.PricePoints.FirstOrDefault(p => p.Date == systemContext.CurrentDate)?.Price ?? decimal.MaxValue -
                c.PricePoints.FirstOrDefault(p => p.Date == systemContext.CurrentDate.AddDays(1))?.Price ??
                decimal.MaxValue);

        if (tradeListing == null)
        {
            return;
        }

        var pricePoint = tradeListing.PricePoints.FirstOrDefault(p => p.Date == systemContext.CurrentDate);
        if (pricePoint == null)
        {
            return;
        }

        systemContext.BuyStock(this, tradeListing, (int)(systemContext.GetCurrentCash(this) / pricePoint.Price));
    }
}