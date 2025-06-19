using System.Collections.Concurrent;
using NasdaqTrader.Bot.Core;

namespace NasdaqTraderSystem.Core;

public class TraderSystemSimulation
{
    public BlockingCollection<HistoricCompanyRecord> Records = new();

    public List<ITraderBot> Players { get; set; } = new();

    public List<IStockListing> StockListings { get; set; } = new();
    public ConcurrentDictionary<ITraderBot, decimal> BankAccounts { get; set; } = new();
    public ConcurrentDictionary<ITraderBot, List<IHolding>> Holdings { get; set; } = new();
    public ConcurrentDictionary<ITraderBot, List<ITrade>> Trades { get; set; } = new();
    public ConcurrentDictionary<ITraderBot, TraderSystemContext> Contexts { get; set; } = new();
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public BlockingCollection<ITraderBot> DidNotFinished { get; set; }


    public TraderSystemSimulation(List<ITraderBot> players, decimal startingCash,
        DateOnly from, DateOnly to,
        int amountOfTradesPerDay, StockLoader stocksLoader)
    {
        DidNotFinished = new();
        Players = players;
        foreach (var player in Players)
        {
            BankAccounts.TryAdd(player, startingCash);
            Holdings.TryAdd(player, new());
            Trades.TryAdd(player, new());

            var systemContext = new TraderSystemContext(this);
            systemContext.CurrentDate = from;
            systemContext.StartDate = from;
            systemContext.EndDate = to;
            systemContext.AmountOfTradesPerDay = amountOfTradesPerDay;
            Contexts.TryAdd(player, systemContext);
        }

        StartDate = from;
        EndDate = to;
        StockListings = stocksLoader.GetListings(from, to);
        stocksLoader.Dispose();
    }

    public async Task<bool> DoSimulationStep(ITraderBot playerBot)
    {
        if (DidNotFinished.Contains(playerBot))
        {
            return false;
        }
        TraderSystemContext systemContext = Contexts[playerBot];
        if (systemContext.CurrentDate >= systemContext.EndDate)
        {
            return false;
        }

        systemContext.CurrentDate = systemContext.CurrentDate.AddDays(1);
        while (systemContext.CurrentDate.DayOfWeek == DayOfWeek.Saturday ||
               systemContext.CurrentDate.DayOfWeek == DayOfWeek.Sunday || systemContext.CurrentDate.IsFederalHoliday())
        {
            systemContext.CurrentDate = systemContext.CurrentDate.AddDays(1);
        }

        try
        {
            await playerBot.DoTurn(systemContext);
        }
        catch (Exception e)
        {
        }

        SaveRecordForDate(systemContext, playerBot);
        return true;
    }

    private void SaveRecordForDate(TraderSystemContext context, ITraderBot player)
    {
        Records.Add(new HistoricCompanyRecord()
        {
            Name = player.CompanyName,
            OnDate = context.CurrentDate,
            Holdings = Holdings[player].OfType<Holding>().Select(c => c.Copy()).ToArray(),
            Cash = BankAccounts[player],
            Transactions = Trades[player].Where(b => b.ExecutedOn == context.CurrentDate).ToArray()
        });
    }


    public decimal GetPriceOnDay(IStockListing listing, DateOnly currentDate)
    {
        var pricePoints = StockListings.First(b => b == listing).PricePoints;
        return pricePoints.FirstOrDefault(p => p.Date == currentDate)?
            .Price ?? 0.0m;
    }

    public bool ProcessTrade(ITraderBot traderBot, Trade trade, TraderSystemContext context)
    {
        if (Trades[traderBot].Count(c => c.ExecutedOn == context.CurrentDate) >= context.AmountOfTradesPerDay)
        {
            return false;
        }

        if (trade.Listing.PricePoints.All(c => c.Date != context.CurrentDate))
        {
            return false;
        }

        var cash = BankAccounts[traderBot];

        if (cash < trade.Amount * trade.AtPrice)
        {
            return false;
        }

        var holding = Holdings[traderBot].FirstOrDefault(b => b.Listing == trade.Listing) as Holding;
        if (holding == null)
        {
            holding = new Holding()
            {
                Listing = trade.Listing
            };
            Holdings[traderBot].Add(holding);
        }

        if (holding.Amount + trade.Amount > 1000)
        {
            return false;
        }
        
        if (trade.Amount < 0
            && holding.Amount + trade.Amount < 0)
        {
            return false;
        }

        holding.Amount += trade.Amount;
        BankAccounts[traderBot] -= (trade.Amount * trade.AtPrice);
        Trades[traderBot].Add(trade);
        return false;
    }
}