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
        
        var tradeListings = listings
            .OrderBy(c =>
                c.PricePoints.FirstOrDefault(p => p.Date == systemContext.CurrentDate)?.Price ?? decimal.MaxValue -
                c.PricePoints.FirstOrDefault(p => p.Date == systemContext.CurrentDate.AddDays(1))?.Price ?? decimal.MaxValue).Take(2);

        foreach (var listing in tradeListings)
        {
            systemContext.BuyStock(this, listing, 5);
        }

    }
}