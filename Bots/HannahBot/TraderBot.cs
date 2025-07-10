using System.Diagnostics;
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
    private Stopwatch stopwatch = new Stopwatch();

    public string CompanyName => "Hannah's Funky Algos Inc.";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        LookAhead += 1;
        if (!initialized)
        {
            stopwatch.Restart();
            Logger.Log("Initializing Hannah's Funky Algos Inc. Bot...");
            initialized = true;
            InitialCash = systemContext.GetCurrentCash(this);
            Opportunities = OpportunisticElectricRock.GetAllOpportunities(systemContext.GetListings());
            Logger.Log($"Found {Opportunities.Count()} opportunities");
            Logger.Log("Done initializing Hannah's Funky Algos Inc. Bot");
        }

        // TODO: Current date is further than end date?
        if (systemContext.EndDate <= systemContext.CurrentDate)
        {
            stopwatch.Stop();
            Logger.Log($"Total time taken: {stopwatch.ElapsedMilliseconds} ms");
        }

        Logger.Log($"Going to try trading for day {systemContext.CurrentDate} with {ActiveOpportunities.Count} active opportunities and {systemContext.GetCurrentCash(this)} cash");
        Logger.Log($"End date is {systemContext.EndDate}, current date is {systemContext.CurrentDate}, trades left for today: {systemContext.GetTradesLeftForToday(this)}");
        var activeOpportunitiesEndingToday = ActiveOpportunities.Where(o => o.SellDate <= systemContext.CurrentDate);

        while (systemContext.GetTradesLeftForToday(this) > 0 && activeOpportunitiesEndingToday.Any() && systemContext.EndDate != systemContext.CurrentDate)
        {
            var currentHoldings = systemContext.GetHoldings(this);
            var activeOpportunity = activeOpportunitiesEndingToday.First();
            var matchingHolding = currentHoldings.First(h => h.Listing.Ticker == activeOpportunity.Listing.Ticker);

            Logger.Log($"Sold {matchingHolding.Listing.Ticker}");
            systemContext.SellStock(this, matchingHolding.Listing, matchingHolding.Amount);
            ActiveOpportunities.Remove(activeOpportunity);
        }

        var remainingCash = systemContext.GetCurrentCash(this);
        if (remainingCash < InitialCash)
        {
            Logger.Log("Not enough cash to trade, skipping day");
            return;
        }

        if (systemContext.GetTradesLeftForToday(this) == 0)
        {
            Logger.Log("No trades left for today, skipping day");
            return;
        }

        var opportunities = Opportunities
            .Where(o => o.BuyDate == systemContext.CurrentDate)
            .Where(o => o.SellDate.DayNumber - o.BuyDate.DayNumber <= LookAhead)
            .OrderByDescending(o => o.StaticScore)
            .ToList();

        if (opportunities.Count == 0)
        {
            Logger.Log("No opportunities found, skipping day");
            return;
        }
        else
        {
            Logger.Log($"Found {opportunities.Count} opportunities");
        }

        while (systemContext.GetTradesLeftForToday(this) > 0 && remainingCash > 10 && opportunities.Count > 0)
        {
            var currentHoldings = systemContext.GetHoldings(this);
            var opportunity = opportunities.First();

            var amountToBuy = Math.Min(1000, (int)(remainingCash / systemContext.GetPriceOnDay(opportunity.Listing)));
            var success = systemContext.BuyStock(this, opportunity.Listing, amountToBuy);
            if (!success)
            {
                Logger.Log($"Failed to buy {opportunity.Listing.Ticker} for {amountToBuy} shares at price {systemContext.GetPriceOnDay(opportunity.Listing)} with cash {remainingCash} for sell date {opportunity.SellDate}");
            }
            else
            {
                ActiveOpportunities.Add(opportunity);
                Logger.Log($"Bought {opportunity.Listing.Ticker} for {amountToBuy} shares at price {systemContext.GetPriceOnDay(opportunity.Listing)} with cash {remainingCash}");
            }

            remainingCash = systemContext.GetCurrentCash(this);
            opportunities.RemoveAll(o => o.Listing.Ticker == opportunity.Listing.Ticker);
        }
    }
}
