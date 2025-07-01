using NasdaqTrader.Bot.Core;

namespace HannahBot;

public class Opportunity(
    IStockListing listing,
    DateOnly buyDate,
    DateOnly sellDate
)
{
    public decimal ProfitPerShare => Listing.PricePoints.FirstOrDefault(p => p.Date == SellDate)!.Price - Listing.PricePoints.FirstOrDefault(p => p.Date == BuyDate)!.Price;
    public decimal BuyPrice => Listing.PricePoints.FirstOrDefault(p => p.Date == BuyDate)!.Price;
    public decimal SellPrice => Listing.PricePoints.FirstOrDefault(p => p.Date == SellDate)!.Price;
    public decimal TradeDuration => SellDate.DayNumber - BuyDate.DayNumber;
    public decimal Score => ProfitPerShare * Math.Max(1, Listing.PricePoints.Count() - TradeDuration);

    public IStockListing Listing { get; } = listing;
    public DateOnly BuyDate { get; } = buyDate;
    public DateOnly SellDate { get; } = sellDate;
}

internal class OpportunisticElectricRock
{
    public static IEnumerable<Opportunity> GetAllOpportunities(IEnumerable<IStockListing> listings)
    {
        IEnumerable<Opportunity> opportunities = [];
        foreach (var listing in listings)
            opportunities = opportunities.Concat(OpportunityForListing(listing));

        return opportunities;
    }

    private static List<Opportunity> OpportunityForListing(IStockListing listing)
    {
        var numberOfDays = listing.PricePoints.Length;
        var opportunities = new List<Opportunity>();
        // Console.WriteLine($"calculating opportunities for {listing.Name} for {numberOfDays} days with look ahead of {LookAheadDays} days");
        for (int buyDay = 0; buyDay < numberOfDays; buyDay++)
        {
            // Console.WriteLine($"Checking for buy day {buyDay}");
            var currentBuyPricePoint = listing.PricePoints[buyDay];
            if (currentBuyPricePoint.Price <= 0)
                continue;

            for (int sellDay = buyDay + 1; sellDay < numberOfDays; sellDay++)
            {
                var currentSellPricePoint = listing.PricePoints[sellDay];

                var profit = currentSellPricePoint.Price - currentBuyPricePoint.Price;

                if (profit > 0)
                    opportunities.Add(new Opportunity(
                        listing,
                        currentBuyPricePoint.Date,
                        currentSellPricePoint.Date
                    ));
            }
        }

        Console.WriteLine($"Found {opportunities.Count} opportunities for stock {listing.Name}");
        return opportunities;
    }
}
