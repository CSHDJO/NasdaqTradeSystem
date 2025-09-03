namespace YurritBot.Logging;

public class NullLogger : ILogger
{
    public void Erase() { }
    public void Initialize() { }
    public void Log(string text) { }
    public void LogHeader1(string text) { }
    public void LogHeader2(string text) { }
    public void LogTransaction(string category, string ticker, decimal currentCash, decimal pricePoint, int amount) { }
}
