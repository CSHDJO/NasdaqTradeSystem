using System.Linq;
namespace NasdaqTrader.Bot.Core;

public static class Extensions
{
    public static decimal GetCurrentValue(this List<IHolding> holdings, DateOnly dateOnly)
    {
        return holdings.Sum(h => h.Listing.PricePoints.FirstOrDefault(c => c.Date == dateOnly)?.Price ?? 0m * h.Amount);
    }
}