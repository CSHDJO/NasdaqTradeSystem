using DennisMoneyBot.Utility;

using NasdaqTrader.Bot.Core;

namespace DennisMoneyBot.Traders;
internal class DayTrader : ITrader
{
    public void DoTrades(ITraderSystemContext systemContext, ITraderBot bot)
    {
        var today = systemContext.CurrentDate;
        var nextTradingDay = DateCalculator.GetNextValidDate(today);
        var currentHoldings = systemContext.GetHoldings(bot);

        foreach (var holding in currentHoldings)
        {
            systemContext.SellStock(bot, holding.Listing, holding.Amount);
        }

        if (nextTradingDay.Year != today.Year)
        {
            return;
        }

        var bestOneDayTrades = systemContext.GetListings()
            .OrderByDescending(listing =>
                listing.PricePoints.FirstOrDefault(p => p.Date == nextTradingDay)?.Price - listing.PricePoints.FirstOrDefault(p => p.Date == today)?.Price);

        int tradesLeft = 2;
        foreach (var listing in bestOneDayTrades)
        {
            int amountToBuy = (int)Math.Min(1000, Math.Floor(systemContext.GetCurrentCash(bot) / listing.PricePoints.First(p => p.Date == today).Price));
            if (amountToBuy == 0)
            {
                continue;
            }

            systemContext.BuyStock(bot, listing, amountToBuy);

            if (--tradesLeft < 1)
            {
                break;
            }
        }
    }
}
