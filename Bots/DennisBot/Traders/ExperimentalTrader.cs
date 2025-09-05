using DennisMoneyBot.Utility;

using NasdaqTrader.Bot.Core;

namespace DennisMoneyBot.Traders;
internal class ExperimentalTrader : ITrader
{
    public void DoTrades(ITraderSystemContext systemContext, ITraderBot bot)
    {
        int windowSize = 1;
        decimal saleCutoffRatio = 1.15M;
        int daysToIgnoreSaleRules = 14;
        int minimalSaleValue = 800;

        var today = systemContext.CurrentDate;
        var nextTradingDay = DateCalculator.GetBusinessDaysInTheFuture(today, windowSize);
        IHolding[]? currentHoldings = systemContext.GetHoldings(bot);

        foreach (var holding in currentHoldings)
        {
            if ((PriceCalculator.GetValueOfHoldingOnDate(holding, today) > minimalSaleValue)
                &&
                (PriceCalculator.GetPricePointRatio(holding.Listing, today, nextTradingDay) < saleCutoffRatio
                || (systemContext.StartDate.AddDays(daysToIgnoreSaleRules) > systemContext.CurrentDate)))
            {
                systemContext.SellStock(bot, holding.Listing, holding.Amount);
            }
        }

        var bestOneDayTrades = systemContext.GetListings()
            .OrderByDescending(listing =>
                PriceCalculator.CalculateProfitMargin(systemContext, bot, listing, today, nextTradingDay));

        int tradesLeft = systemContext.GetTradesLeftForToday(bot);
        foreach (var listing in bestOneDayTrades)
        {
            int amountToBuy = (int)Math.Min(1000, Math.Floor(systemContext.GetCurrentCash(bot) / PriceCalculator.GetPriceForListingOnDate(listing, today)));
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
        // nog iets doen met voorrang geven aan dingen die gewoon veel waard zijn en niet meer gaan dalen, zodat dat kan accumuleren
    }
}
