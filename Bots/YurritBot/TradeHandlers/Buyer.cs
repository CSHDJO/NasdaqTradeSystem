using NasdaqTrader.Bot.Core;
using System.Diagnostics;
using YurritBot.Logging;

namespace YurritBot.TradeHandlers;

public class Buyer(ITraderBot traderBot, ITraderSystemContext systemContext, ILogger logger) : ITradeHandler
{
    private readonly ITraderBot traderBot = traderBot;
    private readonly ITraderSystemContext systemContext = systemContext;
    private readonly ILogger logger = logger;

    private const decimal cashlowerLimit = 100;
    private const int maximumBuyAmountPerStock = 1000;

    public void ExecuteStrategy()
    {
        logger.LogHeader2($"Buy");

        logger.Log($"€{systemContext.GetCurrentCash(traderBot):F2}");

        logger.Log($"Ranking");
        Stopwatch stopwatch = Stopwatch.StartNew();
        var rankedListings = RankListingsByExpectedIncrease(traderBot, systemContext, logger);
        stopwatch.Stop();
        logger.Log($"- took {stopwatch.Elapsed}");

        logger.Log($"Buying");
        var buyCalculator = new BuyCalculator(maximumBuyAmountPerStock);
        foreach (var listing in rankedListings)
        {
            if (systemContext.GetCurrentCash(traderBot) < cashlowerLimit)
            {
                logger.Log($"- cash below limit");
                return;
            }

            if (systemContext.GetTradesLeftForToday(traderBot) <= 0)
            {
                logger.Log($"- no trades left");
                return;
            }

            TryBuyListing(traderBot, systemContext, logger, buyCalculator, listing);
        }
    }

    private static IEnumerable<IStockListing> RankListingsByExpectedIncrease(ITraderBot traderBot, ITraderSystemContext systemContext, ILogger logger)
    {
        var currentCash = systemContext.GetCurrentCash(traderBot);

        return systemContext.GetListings()
            .Select(l =>
            {
                DateOnly[] dateArray = Array.ConvertAll(l.PricePoints, p => p.Date);
                int indexToday = Array.BinarySearch(dateArray, systemContext.CurrentDate);
                if (indexToday < 0 || indexToday >= l.PricePoints.Length -1)
                {
                    return null;
                }

                var priceToday = l.PricePoints[indexToday].Price;
                var priceTomorrow = l.PricePoints[indexToday + 1].Price;
                return new
                {
                    Listing = l,
                    PriceToday = priceToday,
                    PriceRatio = priceTomorrow / priceToday
                };
            })
            .Where(x => x != null
                        && x.PriceToday > 0.0001m
                        && x.PriceToday <= currentCash
                        && x.PriceRatio > 1)
            .OrderByDescending(x => x.PriceRatio)
            .Select(x => x.Listing);
    }

    private static void TryBuyListing(ITraderBot traderBot, ITraderSystemContext systemContext, ILogger logger,
        BuyCalculator buyCalculator, IStockListing listing)
    {
        var price = systemContext.GetPriceOnDay(listing);
        var maxBuyAmount = buyCalculator.CalculateMaximuumBuyAmount(
            currentCash: systemContext.GetCurrentCash(traderBot),
            listingPrice: price);
        var success = systemContext.BuyStock(traderBot, listing, maxBuyAmount);

        logger.LogTransaction(
            category: success ? "- BUY" : "- BUY ERR",
            ticker: listing.Ticker,
            currentCash: systemContext.GetCurrentCash(traderBot),
            pricePoint: price,
            amount: maxBuyAmount);
    }
}
