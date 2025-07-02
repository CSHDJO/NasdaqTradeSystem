using NasdaqTrader.Bot.Core;

namespace HannahBot;

public class TraderBot : ITraderBot
{
    public const int MaxSharesPerListing = 1000;
    private bool initialized = false;
    public IEnumerable<Opportunity> Opportunities { get; set; } = [];
    public List<Opportunity> ActiveOpportunities { get; set; } = [];
    private int LookAhead = 0;
    private decimal InitialCash = 0;

    public string CompanyName => "Hannah's Funky Algos Inc.";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        LookAhead += 1;
        if (!initialized)
        {
            Console.WriteLine("Initializing Hannah's Funky Algos Inc. Bot...");
            initialized = true;
            InitialCash = systemContext.GetCurrentCash(this);
            Opportunities = OpportunisticElectricRock.GetAllOpportunities(systemContext.GetListings());
            Console.WriteLine($"Found {Opportunities.Count()} opportunities");

            // Opportunities
            //     .Take(5)
            //     .ToList()
            //     .ForEach(o =>
            //     {
            //         Console.WriteLine($"Opportunity identified:\nbuy price: {o.BuyPrice}\nsell price: {o.SellPrice}\nprofit: {o.ProfitPerShare}\nduration: {o.TradeDuration}\n");
            //     });
            Console.WriteLine("Done initializing Hannah's Funky Algos Inc. Bot");
        }

        // DayIndex += 1;
        // if (DayIndex < 10)
        // {
        //     ShortTermCapitalGain(systemContext);
        //     return;
        // }
        // else if (DayIndex == 10)
        // {
        //     foreach (var holding in systemContext.GetHoldings(this))
        //     {
        //         systemContext.SellStock(this, holding.Listing, holding.Amount);
        //     }
        //     Console.WriteLine($"Capital after RCG: {systemContext.GetCurrentCash(this)}");
        //     return;
        // }

        Console.WriteLine($"Going to try trading for day {systemContext.CurrentDate} with {ActiveOpportunities.Count} active opportunities and {systemContext.GetCurrentCash(this)} cash");
        var activeOpportunitiesEndingToday = ActiveOpportunities.Where(o => o.SellDate <= systemContext.CurrentDate);

        while (systemContext.GetTradesLeftForToday(this) > 0 && activeOpportunitiesEndingToday.Any() && systemContext.EndDate != systemContext.CurrentDate)
        {
            var currentHoldings = systemContext.GetHoldings(this);
            var activeOpportunity = activeOpportunitiesEndingToday.First();
            var matchingHolding = currentHoldings.First(h => h.Listing.Ticker == activeOpportunity.Listing.Ticker);

            Console.WriteLine($"Sold {matchingHolding.Listing.Ticker}");
            systemContext.SellStock(this, matchingHolding.Listing, matchingHolding.Amount);
            ActiveOpportunities.Remove(activeOpportunity);
        }

        var remainingCash = systemContext.GetCurrentCash(this);
        if (remainingCash < InitialCash)
        {
            Console.WriteLine("Not enough cash to trade, skipping day");
            return;
        }

        var opportunities = Opportunities
            .Where(o => o.BuyDate == systemContext.CurrentDate)
            .Where(o => o.SellDate.DayNumber - o.BuyDate.DayNumber <= LookAhead)
            .OrderByDescending(o => o.Score(remainingCash))
            .ToList();


        if (opportunities.Count == 0)
        {
            Console.WriteLine("No opportunities found, skipping day");
            return;
        }
        else
        {
            Console.WriteLine($"Found {opportunities.Count} opportunities");
        }

        while (systemContext.GetTradesLeftForToday(this) > 0 && remainingCash > 10 && opportunities.Count > 0)
        {
            var currentHoldings = systemContext.GetHoldings(this);
            var opportunity = opportunities.First();

            var amountToBuy = Math.Min(1000, (int)(remainingCash / systemContext.GetPriceOnDay(opportunity.Listing)));
            var success = systemContext.BuyStock(this, opportunity.Listing, amountToBuy);
            if (!success)
            {
                Console.WriteLine($"Failed to buy {opportunity.Listing.Ticker} for {amountToBuy} shares at price {systemContext.GetPriceOnDay(opportunity.Listing)} with cash {remainingCash} for sell date {opportunity.SellDate}");
            }
            else
            {
                ActiveOpportunities.Add(opportunity);
                Console.WriteLine($"Bought {opportunity.Listing.Ticker} for {amountToBuy} shares at price {systemContext.GetPriceOnDay(opportunity.Listing)} with cash {remainingCash}");
            }

            remainingCash = systemContext.GetCurrentCash(this);
            opportunities.RemoveAll(o => o.Listing.Ticker == opportunity.Listing.Ticker);
        }
    }

    private void ShortTermCapitalGain(ITraderSystemContext systemContext)
    {
        foreach (var holding in systemContext.GetHoldings(this))
        {
            systemContext.SellStock(this, holding.Listing, holding.Amount);
        }

        IStockListing oneDayMax = systemContext
            .GetListings()
            .Where(l => l.PricePoints.Any(p => p.Date == systemContext.CurrentDate) && l.PricePoints.Any(p => p.Date == systemContext.CurrentDate.AddDays(1)))
            .MaxBy(l =>
                l.PricePoints.First(p => p.Date == systemContext.CurrentDate.AddDays(1)).Price
                - l.PricePoints.First(p => p.Date == systemContext.CurrentDate).Price
            );

        if (oneDayMax == null)
        {
            return;
        }

        var listing = oneDayMax?.PricePoints.FirstOrDefault(p => p.Date == systemContext.CurrentDate);
        if (listing == null)
        {
            return;
        }

        var amount = (int)(systemContext.GetCurrentCash(this) / listing.Price);
        systemContext.BuyStock(this, oneDayMax!, Math.Min(1000, amount));
    }
}
