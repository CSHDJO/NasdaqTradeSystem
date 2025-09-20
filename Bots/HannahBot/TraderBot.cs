#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using System.Diagnostics;
using NasdaqTrader.Bot.Core;

namespace HannahBot;

public class TraderBot : ITraderBot
{
    public const int MaxSharesPerListing = 1000;
    private bool initialized = false;
    public required IDictionary<DateOnly, List<Opportunity>> Opportunities { get; set; }
    public IDictionary<DateOnly, List<Opportunity>> ActiveOpportunities { get; set; } = new Dictionary<DateOnly, List<Opportunity>>();
    private int LookAhead = 0;
    private decimal InitialCash = 0;
    private int DefaultTradesLeftToday = 0;
    private readonly Stopwatch stopwatch = new();

    public string CompanyName => "Hannah's Funky Algos Inc.";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        LookAhead += 1;
        if (!initialized)
        {
#if DEBUG
            stopwatch.Restart();
#endif
            Logger.Log("Initializing Hannah's Funky Algos Inc. Bot...");
            initialized = true;
            DefaultTradesLeftToday = systemContext.GetTradesLeftForToday(this);
            InitialCash = systemContext.GetCurrentCash(this);
            Opportunities = OpportunisticElectricRock.GetAllOpportunities(systemContext.GetListings())
                .GroupBy(o => o.BuyDate)
                .ToDictionary(g => g.Key, g => g.ToList());

            Logger.Log($"Found {Opportunities.Count} opportunities");
        }

#if DEBUG
        // TODO: Current date is further than end date?
        if (systemContext.EndDate <= systemContext.CurrentDate)
        {
            stopwatch.Stop();
            Logger.Log($"Total time taken: {stopwatch.ElapsedMilliseconds} ms");
        }
#endif
        var remainingCash = systemContext.GetCurrentCash(this);

        Logger.Log($"Going to try trading for day {systemContext.CurrentDate} with {ActiveOpportunities.Count} active opportunities and {remainingCash} cash");
        var tradesLeftToday = DefaultTradesLeftToday;

        if (ActiveOpportunities.TryGetValue(systemContext.CurrentDate, out var activeOpportunitiesToday))
        {
            while (tradesLeftToday > 0 && activeOpportunitiesToday.Count != 0 && systemContext.EndDate != systemContext.CurrentDate)
            {
                var currentHoldings = systemContext.GetHoldings(this);
                var activeOpportunity = activeOpportunitiesToday[0];
                activeOpportunitiesToday.RemoveAt(0);

                var matchingHolding = currentHoldings.First(h => h.Listing.Ticker == activeOpportunity.Listing.Ticker);

                Logger.Log($"Sold {matchingHolding.Listing.Ticker}");
                systemContext.SellStock(this, matchingHolding.Listing, matchingHolding.Amount);
                tradesLeftToday -= 1;
                remainingCash = systemContext.GetCurrentCash(this);
                activeOpportunitiesToday.Remove(activeOpportunity);
            }

        }


        if (remainingCash < InitialCash)
        {
            Logger.Log("Not enough cash to trade, skipping day");
            return;
        }

        if (tradesLeftToday == 0)
        {
            Logger.Log("No trades left for today, skipping day");
            return;
        }

        List<Opportunity> opportunities = [];
        if (Opportunities.TryGetValue(systemContext.CurrentDate, out var todaysOpportunities))
        {

            opportunities = todaysOpportunities
                .Where(o => o.TradeDuration <= LookAhead)
                .OrderByDescending(o => o.StaticScore)
                .ToList();
        }
        else
        {
            Logger.Log($"No opportunities found for date {systemContext.CurrentDate}");
            return;
        }

        if (opportunities.Count == 0)
        {
            Logger.Log("No opportunities found, skipping day");
            return;
        }
        else
        {
            Logger.Log($"Found {opportunities.Count} opportunities");
        }

        while (tradesLeftToday > 0 && remainingCash > 10 && opportunities.Count > 0)
        {
            var opportunity = opportunities[0];
            var amountToBuy = Math.Min(1000, (int)(remainingCash / opportunity.BuyPrice));
            var success = systemContext.BuyStock(this, opportunity.Listing, amountToBuy);
            tradesLeftToday -= 1;
            if (!success)
            {
                Logger.Log($"Failed to buy {opportunity.Listing.Ticker} for {amountToBuy} shares at price {systemContext.GetPriceOnDay(opportunity.Listing)} with cash {remainingCash} for sell date {opportunity.SellDate}");
            }
            else
            {
                ActiveOpportunities.TryGetValue(opportunity.SellDate, out var activeOpportunitiesOnSellDate);
                if (activeOpportunitiesOnSellDate == null)
                {
                    activeOpportunitiesOnSellDate = [opportunity];
                    ActiveOpportunities[opportunity.SellDate] = activeOpportunitiesOnSellDate;
                }
                else
                {
                    activeOpportunitiesOnSellDate!.Add(opportunity);
                }


                Logger.Log($"Bought {opportunity.Listing.Ticker} for {amountToBuy} shares at price {systemContext.GetPriceOnDay(opportunity.Listing)} with cash {remainingCash}");
            }

            remainingCash = systemContext.GetCurrentCash(this);
            opportunities.RemoveAll(o => o.Listing.Ticker == opportunity.Listing.Ticker);
        }
    }
}
