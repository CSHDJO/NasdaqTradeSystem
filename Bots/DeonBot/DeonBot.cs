using NasdaqTrader.Bot.Core;

namespace ExampleTraderBot;

public class DeonBot : ITraderBot
{
    public string CompanyName => "BinomialSolutions";

    public static int Day = 0;

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        //foreach (var holding in systemContext.GetHoldings(this))
        //{
        //    Random random = new Random();
        //    systemContext.SellStock(this, holding.Listing, random.Next(1, holding.Amount));
        //}

        //var listings = systemContext.GetListings();

        //for (int i = 0; i < 100; i++)
        //{
        //    Random random = new Random();
        //    systemContext.BuyStock(this, listings[random.Next(0, listings.Count)], random.Next(1, 5));
        //}

        if (Day < 120)
        {
            DoHendrikStuff(systemContext);
        }
        else
        {
            DoBankruptStuff(systemContext);
        }
        Day++;
    }

    public void DoHendrikStuff(ITraderSystemContext systemContext)
    {
        var listings = systemContext.GetListings();

        foreach (var holding in systemContext.GetHoldings(this))
        {
            systemContext.SellStock(this, holding.Listing, holding.Amount);
        }

        var tradeListing = listings
            .Where(c => c.PricePoints.Any(p => p.Date == systemContext.CurrentDate) && c.PricePoints.Any(p => p.Date == systemContext.CurrentDate.AddDays(1)))
            .MaxBy(c =>
                c.PricePoints.FirstOrDefault(p => p.Date == systemContext.CurrentDate.AddDays(1))?.Price -
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

    public void DoBankruptStuff(ITraderSystemContext systemContext)
    {

        var listings = systemContext.GetListings();
        var now = systemContext.CurrentDate;

        foreach (var holding in systemContext.GetHoldings(this))
        {
            systemContext.SellStock(this, holding.Listing, holding.Amount);
        }

        var candidates = listings
            .Where(c =>
                c.PricePoints.Any(p => p.Date == now) &&
                c.PricePoints.Any(p => p.Date == now.AddDays(1)))
            .Select(c => new
            {
                Listing = c,
                PriceNow = c.PricePoints.First(p => p.Date == now).Price,
                PriceFuture = c.PricePoints.First(p => p.Date == now.AddDays(1)).Price
            })
            .Where(x => x.PriceFuture < x.PriceNow)
            .OrderBy(x => x.PriceFuture - x.PriceNow)
            .FirstOrDefault();

        if (candidates == null)
        {
            return;
        }

        systemContext.BuyStock(this, candidates.Listing, (int)(systemContext.GetCurrentCash(this) / candidates.PriceNow));
    }   
}