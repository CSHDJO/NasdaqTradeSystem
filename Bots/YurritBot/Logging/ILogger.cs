namespace YurritBot.Logging;

public interface ILogger
{
    void Log(string text);
    void LogTransaction(string category, string ticker, decimal currentCash, decimal pricePoint, int amount);
}