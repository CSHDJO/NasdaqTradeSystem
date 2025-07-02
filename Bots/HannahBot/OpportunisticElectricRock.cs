using NasdaqTrader.Bot.Core;

namespace HannahBot;

public class Opportunity
{
    public Opportunity(
    IStockListing listing,
    DateOnly buyDate,
    DateOnly sellDate
)
    {
        Listing = listing;
        BuyDate = buyDate;
        SellDate = sellDate;
        ProfitPerShare = Listing.PricePoints.FirstOrDefault(p => p.Date == SellDate)!.Price - Listing.PricePoints.FirstOrDefault(p => p.Date == BuyDate)!.Price;
        BuyPrice = Listing.PricePoints.FirstOrDefault(p => p.Date == BuyDate)!.Price;
        SellPrice = Listing.PricePoints.FirstOrDefault(p => p.Date == SellDate)!.Price;
        TradeDuration = SellDate.DayNumber - BuyDate.DayNumber;
    }

    public decimal ProfitPerShare;
    public decimal BuyPrice;
    public decimal SellPrice;
    public decimal TradeDuration;
	public decimal Score(decimal currentCash)
	{
		var maxAffordableShares = Math.Min(1000, (int)(currentCash / BuyPrice));
		if (maxAffordableShares == 0) return 0;
		
        var totalProfit = ProfitPerShare * maxAffordableShares;
		return totalProfit / (TradeDuration * BuyPrice);
	}

    public IStockListing Listing { get; }
    public DateOnly BuyDate { get; }
    public DateOnly SellDate { get; }
}

internal class OpportunisticElectricRock
{
    public static IEnumerable<Opportunity> GetAllOpportunities(IEnumerable<IStockListing> listings)
    {
        return listings.AsParallel()
            .SelectMany(OpportunityForListing)
            .ToList();
    }

    private static List<Opportunity> OpportunityForListing(IStockListing listing)
    {
        var pricePoints = listing.PricePoints;
        var numberOfDays = listing.PricePoints.Length;
        var opportunities = new List<Opportunity>();
        
        for (int buyDay = 0; buyDay < numberOfDays; buyDay++)
        {
            var buyPoint = pricePoints[buyDay];
            if (buyPoint.Price <= 0)
                continue;

            decimal maxSellPrice = buyPoint.Price;
            for (int sellDay = buyDay + 1; sellDay < numberOfDays; sellDay++)
            {
                var sellPoint = pricePoints[sellDay];
                if (sellPoint.Price <= 0)
                    continue;

                if (sellPoint.Price > buyPoint.Price)
                    opportunities.Add(new Opportunity(
                        listing,
                        buyPoint.Date,
                        sellPoint.Date
                    ));
            }
        }

#if DEBUG
        Console.WriteLine($"Found {opportunities.Count} opportunities for stock {listing.Name}");
#endif
        return opportunities;
    }
}
