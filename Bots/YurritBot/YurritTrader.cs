using NasdaqTrader.Bot.Core;
using YurritBot.Logging;
using YurritBot.TradeHandlers;

namespace YurritBot;

public class YurritBot : ITraderBot
{
    public string CompanyName => "Stock Out Like a Sore Thumb";
    private const string logFileName = "yurritbot_log.html";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        //var logger = new HtmlLogger(Path.Combine(AppContext.BaseDirectory, logFileName));
        var logger = new NullLogger();

        if (!File.Exists(Path.Combine(AppContext.BaseDirectory, logFileName)))
        {
            logger.Initialize();
        }

        logger.LogHeader1($"{systemContext.CurrentDate}");
        logger.Log($"{DateTime.Now:dd-MMM-yyyy,hh:mm::ss.fff}");

        new Seller(this, systemContext, logger).ExecuteStrategy();
        new Buyer(this, systemContext, logger).ExecuteStrategy();

    }
}