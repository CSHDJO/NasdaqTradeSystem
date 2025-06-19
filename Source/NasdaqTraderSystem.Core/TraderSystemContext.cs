using System.Collections.ObjectModel;
using System.Diagnostics;
using NasdaqTrader.Bot.Core;

namespace NasdaqTraderSystem.Core;

public class TraderSystemContext : ITraderSystemContext
{
    private readonly TraderSystemSimulation _simulation;

    public TraderSystemContext(TraderSystemSimulation simulation)
    {
        _simulation = simulation;
    }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly CurrentDate { get; set; }
    public int AmountOfTradesPerDay { get; set; }

    public decimal GetCurrentCash(ITraderBot traderBot)
    {
        return _simulation.BankAccounts[traderBot];
    }

    public decimal GetPriceOnDay(IStockListing listing)
    {
        return _simulation.GetPriceOnDay(listing, CurrentDate);
    }

    public ReadOnlyCollection<IStockListing> GetListings()
    {
        return _simulation.StockListings.AsReadOnly();
    }

    public int GetTradesLeftForToday(ITraderBot traderBot)
    {
        return AmountOfTradesPerDay - _simulation.Trades[traderBot]
            .Count(c => c.ExecutedOn == CurrentDate);
    }

    public bool BuyStock(ITraderBot traderBot, IStockListing listing, int amount)
    {
        if (amount <= 0)
        {
            return false;
        }
        var currentPrice = GetPriceOnDay(listing);
        Trade trade = new Trade();
        trade.Listing = listing;
        trade.Amount = amount;
        trade.ExecutedOn = CurrentDate;
        trade.AtPrice = currentPrice;
        return _simulation.ProcessTrade(traderBot, trade, this);
    }

    public bool SellStock(ITraderBot traderBot, IStockListing listing, int amount)
    {
        if (amount <= 0)
        {
            return false;
        }
        var currentPrice = GetPriceOnDay(listing);
        Trade trade = new Trade();
        trade.Listing = listing;
        trade.Amount = -amount;
        trade.ExecutedOn = CurrentDate;
        trade.AtPrice = currentPrice;
        return _simulation.ProcessTrade(traderBot, trade, this);
    }

    public IHolding GetHolding(ITraderBot traderBot, IStockListing listing)
    {
        var holding = _simulation.Holdings[traderBot].FirstOrDefault(b => b.Listing == listing);
        if (holding == null)
        {
            holding = new Holding()
            {
                Listing = listing,
                Amount = 0
            };
            _simulation.Holdings[traderBot].Add(holding);
        }
        return holding;
    }

    public IHolding[] GetHoldings(ITraderBot traderBot)
    {
        return _simulation.Holdings[traderBot].ToArray();
    }
}

public class Holding : IHolding
{
    public IStockListing Listing { get; set; }
    public int Amount { get; set; }

    public IHolding Copy()
    {
        return new Holding()
        {
            Listing = Listing,
            Amount = Amount
        };
    }
}