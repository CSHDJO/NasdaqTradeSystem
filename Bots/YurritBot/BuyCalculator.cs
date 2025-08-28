namespace YurritBot;

public class BuyCalculator(int maximumBuyAmountPerStock)
{
    public int MaximumBuyAmountPerStock { get; private set; } = maximumBuyAmountPerStock;

    public int CalculateMaximuumBuyAmount(decimal currentCash, decimal listingPrice)
    {
        decimal maximumBuyAmountWithCurrentCash = currentCash / listingPrice;
        return (int)Math.Floor(Math.Min(maximumBuyAmountPerStock, maximumBuyAmountWithCurrentCash));
    }
}
