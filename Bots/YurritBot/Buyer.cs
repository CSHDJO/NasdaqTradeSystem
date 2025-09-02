using NasdaqTrader.Bot.Core;
using System.Diagnostics;
using YurritBot.Logging;

namespace YurritBot;

public class Buyer()
{
    private const decimal cashlowerLimit = 100;
    private const int maximumBuyAmountPerStock = 1000;

    public void ExecuteStrategy(ITraderBot traderBot, ITraderSystemContext systemContext, ILogger logger)
    {
        logger.LogHeader2($"Buy");

        logger.Log($"€{systemContext.GetCurrentCash(traderBot):F2}");

        var listingsByExpectedIncrease = RankListings(traderBot, systemContext, logger);
        logger.Log($"ranked listings: {listingsByExpectedIncrease.Count()}");

        var buyCalculator = new BuyCalculator(maximumBuyAmountPerStock);

        if (listingsByExpectedIncrease.Count == 0)
        {
            logger.Log($"no listings");
            return;
        }

        foreach (var listing in listingsByExpectedIncrease)
        {
            var currentCash = systemContext.GetCurrentCash(traderBot);
            if (systemContext.GetTradesLeftForToday(traderBot) <= 0)
            {
                logger.Log($"no trades left");
                return;
            }

            if (currentCash < cashlowerLimit)
            {
                logger.Log($"cash below limit");
                return;
            }

            TryBuyListing(traderBot, systemContext, logger, buyCalculator, listing);
        }

        logger.Log($"- €{systemContext.GetCurrentCash(traderBot):F2}");
    }

    private static List<IStockListing> RankListings(ITraderBot traderBot, ITraderSystemContext systemContext, ILogger logger)
    {
        var currentCash = systemContext.GetCurrentCash(traderBot);

        Stopwatch stopwatch = Stopwatch.StartNew();
        var rankedListings = systemContext.GetListings()
            .Select(l =>
            {
                var indexToday = Array.FindIndex(l.PricePoints, 0, l.PricePoints.Length, p => p.Date == systemContext.CurrentDate);
                var priceToday = l.PricePoints[indexToday].Price;
                var priceTomorrow = l.PricePoints[indexToday + 1].Price;
                var priceRatio = priceTomorrow / priceToday;
                return new {
                    Listing = l,
                    PriceToday = priceToday,
                    PriceRatio = priceRatio
                };
            })
            .Where(x => x.PriceToday <= currentCash
                    && x.PriceRatio > 1)
            .OrderByDescending(x => x.PriceRatio)
            .Select(x => x.Listing)
            .ToList();
        stopwatch.Stop();
        logger.Log($"Ranking took {stopwatch.ElapsedTicks} ticks");

        return rankedListings;
    }

    private static void TryBuyListing(ITraderBot traderBot, ITraderSystemContext systemContext, ILogger logger,
        BuyCalculator buyCalculator, IStockListing listing)
    {
        var price = systemContext.GetPriceOnDay(listing);
        var currentCash = systemContext.GetCurrentCash(traderBot);

        // Redundant? Already in Linq
        if (currentCash < systemContext.GetPriceOnDay(listing))
        {
            logger.Log($"too expensive");
            return;
        }

        if (price < 0.0001m)
        {
            logger.Log($"Failed to get price");
            return;
        }

        var maxBuyAmount = buyCalculator.CalculateMaximuumBuyAmount(currentCash, price);

        var success = systemContext.BuyStock(traderBot, listing, maxBuyAmount);

        logger.LogTransaction(
            category: success ? "" : "ERR",
            ticker: listing.Ticker,
            currentCash: systemContext.GetCurrentCash(traderBot),
            pricePoint: price,
            amount: maxBuyAmount);
    }
}
