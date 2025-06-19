using NasdaqTrader.Bot.Core;

namespace AleixBot;

public class AleixBot : ITraderBot
{
    public string CompanyName => "Beginner Investments";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        var listings = systemContext.GetListings();
        var cash = systemContext.GetCurrentCash(this);
        var currentDate = systemContext.CurrentDate;
        var tradesLeft = systemContext.GetTradesLeftForToday(this);

        systemContext.BuyStock(this, listings[0], 1);
    }
}