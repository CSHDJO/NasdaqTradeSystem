namespace YurritBot.Logging;

public class FileLogger : ILogger
{
    public void Log(string text)
    {
        var logFilePath = Path.Combine(AppContext.BaseDirectory, "yurritbot_errors.log");
        File.AppendAllText(logFilePath, $"{text}{Environment.NewLine}");
    }

    public void LogTransaction(string category, string ticker, decimal currentCash, decimal pricePoint, int amount)
    {
        Log($"- {category} {currentCash:F2} | {ticker} {amount} @ {pricePoint:F2}");
    }
}
