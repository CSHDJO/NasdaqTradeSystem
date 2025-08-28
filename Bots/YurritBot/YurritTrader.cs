using NasdaqTrader.Bot.Core;
using System.Diagnostics;
using YurritBot.Logging;

namespace YurritBot;

public class YurritBot : ITraderBot
{
    public string CompanyName => "Stock Out Like a Sore Thumb";
    
    private const int timeScale = 1;

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        var logger = new NullLogger();

        logger.Log($"");
        logger.Log($"{systemContext.CurrentDate}");
        logger.Log($"=========");

        int indexToday = new DateCalculator()
            .DetermineDateIndex(systemContext.CurrentDate, systemContext.GetListings().First().PricePoints);

        logger.Log($"- €{systemContext.GetCurrentCash(this):F2}");

        logger.Log($"SELL");
        logger.Log($"----");
        var timerSell = new Stopwatch();
        timerSell.Start();
        new Seller(this, systemContext, indexToday, indexToday - timeScale, logger)
            .ExecuteStrategy();
        timerSell.Stop();
        logger.Log($"- {timerSell.ElapsedMilliseconds}ms");

        logger.Log($"- €{systemContext.GetCurrentCash(this):F2}");

        logger.Log($"BUY");
        logger.Log($"---");
        var timerBuy = new Stopwatch();
        timerBuy.Start();
        new Buyer(this, systemContext, indexToday, indexToday + timeScale, logger)
            .ExecuteStrategy();
        timerBuy.Stop();
        logger.Log($"- {timerBuy.ElapsedMilliseconds}ms");

        logger.Log($"- €{systemContext.GetCurrentCash(this):F2}");
    }

    
}