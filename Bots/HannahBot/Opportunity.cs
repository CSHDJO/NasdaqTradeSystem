using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NasdaqTrader.Bot.Core;

namespace HannahBot;
public class Opportunity
{
    public Opportunity(
    IStockListing listing,
    IPricePoint buyPoint,
    IPricePoint sellPoint
)
    {
        Listing = listing;
        BuyDate = buyPoint.Date;
        SellDate = sellPoint.Date;
        ProfitPerShare = sellPoint.Price - buyPoint.Price;
        BuyPrice = buyPoint.Price;
        SellPrice = sellPoint.Price;
        TradeDuration = SellDate.DayNumber - BuyDate.DayNumber;
    }

    public decimal ProfitPerShare;
    public decimal BuyPrice;
    public decimal SellPrice;
    public decimal TradeDuration;
	public decimal Score(decimal currentCash)
	{
		var maxAffordableShares = Math.Min(1000, (int)(currentCash / BuyPrice));
		if (maxAffordableShares == 0) return 0;
		
        var totalProfit = ProfitPerShare * maxAffordableShares;
		return totalProfit / (TradeDuration * BuyPrice);
	}

    public IStockListing Listing { get; }
    public DateOnly BuyDate { get; }
    public DateOnly SellDate { get; }
}
