namespace YurritBot.Logging;

public class FileLogger(string logFilePath) : ILogger
{
    private readonly string logFilePath = logFilePath;

    public void Log(string text)
    {
        File.AppendAllText(logFilePath, $"{text}{Environment.NewLine}");
    }

    public void LogTransaction(string category, string ticker, decimal currentCash, decimal pricePoint, int amount)
    {
        Log($"- {category} {currentCash:F2} | {ticker} {amount} @ {pricePoint:F2}");
    }

    public void LogHeader1(string text)
    {
        File.AppendAllLines(
            logFilePath,
            [string.Empty, text, new string('=', text.Length)]);
    }

    public void LogHeader2(string text)
    {
        File.AppendAllLines(
            logFilePath,
            [text, new string('-', text.Length)]);
    }

    public void Erase()
    {
        File.Delete(logFilePath);
    }

    public void Initialize()
    {
        
    }
}
