using NasdaqTrader.Bot.Core;
using System.Reflection;
using YurritBot.Logging;

namespace YurritBot;

class Buyer(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, int indexReferenceDay, ILogger logger) : ITrader
{
    public ITraderBot TraderBot { get; private set; } = traderBot;
    public ITraderSystemContext SystemContext { get; private set; } = systemContext;
    public int IndexToday { get; private set; } = indexToday;
    public int IndexReferenceDay { get; private set; } = indexReferenceDay;
    public ILogger Logger { get; private set; } = logger;

    private const decimal cashlowerLimit = 100;
    private const int maxBuyAmountPerStock = 1000;

    public void ExecuteStrategy()
    {
        var currentCash = SystemContext.GetCurrentCash(TraderBot);
        var listingsByExpectedIncrease = SystemContext.GetListings()
            .Where(listing => listing.PricePoints[IndexToday].Price <= currentCash)
            .Where(listing => listing.PricePoints[IndexReferenceDay].Price / listing.PricePoints[IndexToday].Price > 1)
            .OrderByDescending(listing => (listing.PricePoints[IndexReferenceDay].Price / listing.PricePoints[IndexToday].Price));

        var buyCalculator = new BuyCalculator(maxBuyAmountPerStock);

        foreach (var listing in listingsByExpectedIncrease)
        {
            currentCash = SystemContext.GetCurrentCash(TraderBot);
            if (SystemContext.GetTradesLeftForToday(TraderBot) <= 0
                || currentCash < cashlowerLimit)
            {
                return;
            }

            var maxBuyAmount = buyCalculator.CalculateMaximuumBuyAmount(currentCash, listing.PricePoints[IndexToday].Price);

            if (maxBuyAmount < 1)
            {
                continue;
            }

            var success = SystemContext.BuyStock(TraderBot, listing, maxBuyAmount);

            Logger.LogTransaction(
                category: success ? "" : "ERR",
                ticker: listing.Ticker,
                currentCash: SystemContext.GetCurrentCash(TraderBot),
                pricePoint: listing.PricePoints[IndexToday].Price,
                amount: maxBuyAmount);
        }
    }
}
