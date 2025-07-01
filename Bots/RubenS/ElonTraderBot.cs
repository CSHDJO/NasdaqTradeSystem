using NasdaqTrader.Bot.Core;

namespace ElonTrader;

public class ElonTraderBot : ITraderBot
{
    const int MaxSharesPerStock = 1000;
    private readonly Dictionary<DateOnly, List<IStockListing>> sellSchedule = [];

    public string CompanyName => "ElonTrader &#8482;";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        if (sellSchedule.TryGetValue(systemContext.CurrentDate, out var sells))
        {
            foreach (var listing in sells)
            {
                var holding = systemContext.GetHolding(this, listing);
                systemContext.SellStock(this, holding.Listing, holding.Amount);
                if (systemContext.GetTradesLeftForToday(this) <= 0) break;
            }

            sellSchedule.Remove(systemContext.CurrentDate);
        }

        if (systemContext.GetTradesLeftForToday(this) <= 0) return;

        List<(IStockListing stock, DateOnly sellDay, decimal profitPerShare)> candidates = [];

        foreach (var stock in systemContext.GetListings())
        {
            if (systemContext.GetHolding(this, stock).Amount >= MaxSharesPerStock) continue;

            var todaysPrice = stock.PricePoints.FirstOrDefault(pp => pp.Date == systemContext.CurrentDate)?.Price ?? 0;

            DateOnly bestSellDay = DateOnly.MinValue;
            decimal maxProfit = 0;

            for (DateOnly futureDate = systemContext.CurrentDate.AddDays(1); futureDate < systemContext.EndDate; futureDate.AddDays(1))
            {
                decimal futurePrice = stock.PricePoints.FirstOrDefault(pp => pp.Date == futureDate)?.Price ?? 0;
                decimal profit = futurePrice - todaysPrice;
                if (profit > maxProfit)
                {
                    maxProfit = profit;
                    bestSellDay = futureDate;
                }
            }

            if (bestSellDay != DateOnly.MinValue)
            {
                candidates.Add((stock, bestSellDay, maxProfit));
            }
        }

        var bestTrades = candidates
            .OrderByDescending(t => t.profitPerShare)
            .Take(systemContext.GetTradesLeftForToday(this))
            .ToList();

        foreach (var (stock, sellDay, profit) in bestTrades)
        {
            decimal buyPrice = systemContext.GetPriceOnDay(stock);
            int availableShares = MaxSharesPerStock - systemContext.GetHolding(this, stock).Amount;
            int affordableShares = (int)(systemContext.GetCurrentCash(this) / buyPrice);
            int sharesToBuy = Math.Min(availableShares, affordableShares);

            if (sharesToBuy > 0)
            {
                systemContext.BuyStock(this, stock, sharesToBuy);

                // Schedule sell
                if (!sellSchedule.TryGetValue(sellDay, out List<IStockListing>? scheduledListings))
                {
                    scheduledListings = [];
                    sellSchedule[sellDay] = scheduledListings;
                }

                scheduledListings.Add(stock);

                if (systemContext.GetTradesLeftForToday(this) <= 0) return;
            }
        }
    }
}