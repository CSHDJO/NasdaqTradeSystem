using NasdaqTrader.Bot.Core;
using YurritBot.Logging;

namespace YurritBot;

class Seller(ITraderBot traderBot, ITraderSystemContext systemContext, int indexToday, int indexReferenceDay, ILogger logger) : ITrader
{
    public ITraderBot TraderBot { get; private set; } = traderBot;
    public ITraderSystemContext SystemContext { get; private set; } = systemContext;
    public int IndexToday { get; private set; } = indexToday;
    public int IndexReferenceDay { get; private set; } = indexReferenceDay;
    public ILogger Logger { get; private set; } = logger;

    private const int cashLowerLimit = 10;

    public void ExecuteStrategy()
    {
        var holdings = SystemContext.GetHoldings(TraderBot);
        if (holdings.Length == 0)
        {
            return;
        }

        foreach (var holding in holdings)
        {
            if (holding.Amount == 0)
            {
                continue;
            }

            var success = SystemContext.SellStock(TraderBot, holding.Listing, holding.Amount);

            Logger.LogTransaction(
                category: success ? "" : "ERR",
                ticker: holding.Listing.Ticker,
                currentCash: SystemContext.GetCurrentCash(TraderBot),
                pricePoint: holding.Listing.PricePoints[IndexToday].Price,
                amount: holding.Amount);
        }
    }
}
