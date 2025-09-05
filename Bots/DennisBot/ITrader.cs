using NasdaqTrader.Bot.Core;

namespace DennisMoneyBot;
internal interface ITrader
{
    void DoTrades(ITraderSystemContext systemContext, ITraderBot bot);
}
