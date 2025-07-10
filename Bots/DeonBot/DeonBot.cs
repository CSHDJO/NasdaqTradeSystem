using NasdaqTrader.Bot.Core;

namespace ExampleTraderBot;

public class DeonBot : ITraderBot
{
    public string CompanyName => "BankruptSolutions";

    public static int Day = 0;

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        Day = systemContext.CurrentDate.Day;

        foreach (var holding in systemContext.GetHoldings(this))
        {
            Random random = new Random();
            systemContext.SellStock(this, holding.Listing, random.Next(1, holding.Amount));
        }

        var listings = systemContext.GetListings();

        for (int i = 0; i < 100; i++)
        {
            Random random = new Random();
            systemContext.BuyStock(this, listings[random.Next(0, listings.Count)], random.Next(1, 5));
        }

    }
}