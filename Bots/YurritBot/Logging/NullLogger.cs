namespace YurritBot.Logging;

public class NullLogger : ILogger
{
    public void Log(string text)
    {
        
    }

    public void LogTransaction(string category, string ticker, decimal currentCash, decimal pricePoint, int amount)
    {
        
    }
}
