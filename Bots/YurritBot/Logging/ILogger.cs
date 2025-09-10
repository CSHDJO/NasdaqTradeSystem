namespace YurritBot.Logging;

public interface ILogger
{
    void Log(string text);
    void LogHeader1(string text);
    void LogHeader2(string text);
    void LogTransaction(string category, string ticker, decimal currentCash, decimal pricePoint, int amount);

    void Initialize();
    void Erase();
}