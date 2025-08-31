using NasdaqTrader.Bot.Core;
using YurritBot.Logging;

namespace YurritBot;

public class YurritBot : ITraderBot
{
    public string CompanyName => "Stock Out Like a Sore Thumb";
    private const string logFileName = "yurritbot_log.html";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        var logger = new HtmlLogger(Path.Combine(AppContext.BaseDirectory, logFileName));

        int indexToday = -1;
        try
        {
            // SLOW
            indexToday = new DateCalculator()
                .DetermineDateIndex(systemContext.CurrentDate, systemContext.GetListings().First().PricePoints);
        }
        catch (ArgumentException ex)
        {
            logger.Log($"Error determining date index: {ex.Message}");
            return;
        }

        if (indexToday == 0)
        {
            logger.Erase();
            logger.Initialize();
        }
        logger.LogHeader1($"{systemContext.CurrentDate}");
        logger.Log($"{DateTime.Now:dd-MMM-yyyy,hh:mm::ss.fff}");

        new Seller().ExecuteStrategy(this, systemContext, logger);
        new Buyer().ExecuteStrategy(this, systemContext, logger);

    }


}