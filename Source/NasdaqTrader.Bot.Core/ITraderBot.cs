namespace NasdaqTrader.Bot.Core;

public interface ITraderBot
{
    Task DoTurn(ITraderSystemContext systemContext);
    string CompanyName { get; }
}