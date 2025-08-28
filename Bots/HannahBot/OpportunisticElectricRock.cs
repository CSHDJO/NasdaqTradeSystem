using NasdaqTrader.Bot.Core;

namespace HannahBot;


internal class OpportunisticElectricRock
{
    private const int MAX_LOOKAHEAD = 10;

    public static IEnumerable<Opportunity> GetAllOpportunities(IEnumerable<IStockListing> listings)
    {
        return listings.AsParallel()
            .SelectMany(OpportunityForListing)
            .ToList();
    }

    private static List<Opportunity> OpportunityForListing(IStockListing listing)
    {
        var pricePoints = listing.PricePoints;
        var maxLookAhead = Math.Min(MAX_LOOKAHEAD, pricePoints.Length - 1);
        var numberOfDays = listing.PricePoints.Length;
        var opportunities = new List<Opportunity>();

        for (int buyDay = 0; buyDay < numberOfDays; buyDay++)
        {
            var buyPoint = pricePoints[buyDay];
            if (buyPoint.Price <= 0)
                continue;

            int maxSellDay = Math.Min(buyDay + maxLookAhead, numberOfDays);
            for (int sellDay = buyDay + 1; sellDay < maxSellDay; sellDay++)
            {
                var sellPoint = pricePoints[sellDay];
                if (sellPoint.Price <= 0)
                    continue;

                if (sellPoint.Price > buyPoint.Price)
                    opportunities.Add(new Opportunity(
                        listing,
                        buyPoint,
                        sellPoint
                    ));
            }
        }

        Logger.Log($"Found {opportunities.Count} opportunities for stock {listing.Name}");
        return opportunities;
    }
}
