using System.Collections.ObjectModel;

namespace NasdaqTrader.Bot.Core;

public interface ITraderSystemContext
{
    DateOnly StartDate { get; set; }
    DateOnly EndDate { get; set; }
    DateOnly CurrentDate { get; set; }
    int AmountOfTradesPerDay { get; }

    decimal GetCurrentCash(ITraderBot traderBot);
    decimal GetPriceOnDay(IStockListing listing);
    ReadOnlyCollection<IStockListing> GetListings();
    int GetTradesLeftForToday(ITraderBot traderBot);
    bool BuyStock(ITraderBot traderBot,IStockListing listing, int amount);
    bool SellStock(ITraderBot traderBot, IStockListing listing, int amount);
    IHolding GetHolding(ITraderBot traderBot, IStockListing listing);
    IHolding[] GetHoldings(ITraderBot traderBot);
}