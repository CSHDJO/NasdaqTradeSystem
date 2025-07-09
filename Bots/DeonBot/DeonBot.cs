using NasdaqTrader.Bot.Core;

namespace ExampleTraderBot;

public class DeonBot : ITraderBot
{
    public string CompanyName => "BankruptSolutions";

    public async Task DoTurn(ITraderSystemContext systemContext)
    {
        var listings = systemContext.GetListings();

        for(int i = 0; i < 4; i++)
        {
            Random random = new Random();
            systemContext.BuyStock(this, listings[random.Next(0, listings.Count)], random.Next(1, 10));
        }
    }
}