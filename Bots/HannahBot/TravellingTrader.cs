using NasdaqTrader.Bot.Core;

namespace HannahBot;

// TODO: define space state dimensions
internal class TravellingTrader
{
    readonly IStockListing listing;
    public decimal[,,] BestTradesMap { get; set; }

    public decimal MaxProfit { get; private set; }
    public int MaxShares { get; private set; }
    public int NumberOfDays => listing.PricePoints.Length;
    public int InitialCashForCalculation { get; private set; }

    public int cash_level_discretized = 100;

    public TravellingTrader(IStockListing listing, int maxShares = 1000, int initialCash = 1000)
    {
        this.listing = listing;
        InitialCashForCalculation = initialCash;
        MaxProfit = 0;
        MaxShares = maxShares;
        BestTradesMap = CreateEmptyStateSpace();

        CalculateStateSpace();
    }

    public void CalculateStateSpace()
    {

    }

    private decimal[,,] CreateEmptyStateSpace()
    {
        decimal[,,] dp = new decimal[NumberOfDays, 1000, 5];
        for (int day = 0; day < NumberOfDays; day++)
        {
            for (int cash_interval = 0; cash_interval <= 1000; cash_interval++)
            {
                for (int remaining_trades = 0; remaining_trades <= 5; remaining_trades++)
                {
                    dp[day, cash_interval, remaining_trades] = decimal.MinValue;
                }
            }
        }

        dp[0, 0, 5] = InitialCashForCalculation;
        return dp;
    }
}
