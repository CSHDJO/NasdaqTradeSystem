using static System.Net.Mime.MediaTypeNames;

namespace YurritBot.Logging;

public class HtmlLogger(string logFilePath) : ILogger
{
    private readonly string logFilePath = logFilePath;

    public void Log(string text)
    {
        File.AppendAllText(logFilePath, $"<p>{text}</p>{Environment.NewLine}");
    }

    public void LogTransaction(string category, string ticker, decimal currentCash, decimal pricePoint, int amount)
    {
        Log($"<p>{category} | €{currentCash:F2} | {ticker} {amount} @ {pricePoint:F2}</p>");
    }

    public void LogHeader1(string text)
    {
        Log($"<h1>{text}</h1>");
    }

    public void LogHeader2(string text)
    {
        Log($"<h2>{text}</h2>");
    }

    public void Erase()
    {
        File.Delete(logFilePath);
    }

    public void Initialize()
    {
        File.AppendAllText(logFilePath, $"<head>{Environment.NewLine}");
        File.AppendAllText(logFilePath, $"<link rel=\"stylesheet\" href=\"./Bots/Logging/style.css\">{Environment.NewLine}");
        File.AppendAllText(logFilePath, $"</head>{Environment.NewLine}");
        File.AppendAllText(logFilePath, $"</body>{Environment.NewLine}");
    }
}
