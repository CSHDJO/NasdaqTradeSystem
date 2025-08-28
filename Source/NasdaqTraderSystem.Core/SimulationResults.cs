using NasdaqTrader.Bot.Core;

namespace NasdaqTraderSystem.Core;

public class SimulationResults
{
    public string RunAt { get; set; }
    public IStockListing[] Listings { get; set; } = Array.Empty<IStockListing>();
    public CompanyResult[] Companies { get; set; } = Array.Empty<CompanyResult>();
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal StartCash { get; set; }
    public Dictionary<ITraderBot, List<ITrade>> Trades { get; set; }
    public string GameName { get; set; }
}

public class CompanyResult
{
    public string Name { get; set; } = "";
    public decimal Cash { get; set; }
    public IHolding[] Holdings { get; set; } = Array.Empty<IHolding>();
    public DateOnly OnDate { get; set; }
    public TimeSpan RunDuration { get; set; }
    public decimal HoldingsValue { get; set; }
    public decimal Total { get; set; }
}