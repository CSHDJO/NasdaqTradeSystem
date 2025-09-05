using DennisMoneyBot.Traders;

using NasdaqTrader.Bot.Core;

namespace DennisMoneyBot;

public class MoneyBot : ITraderBot
{
    public string CompanyName => "Dennis Makes It Rain";

    //ITrader _selectedTrader = new DayTrader();
    ITrader _selectedTrader = new ExperimentalTrader();

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        _selectedTrader.DoTrades(systemContext, this);
    }
}