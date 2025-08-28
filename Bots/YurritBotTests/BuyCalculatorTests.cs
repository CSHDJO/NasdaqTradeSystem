using FluentAssertions;
using NUnit.Framework;
using YurritBot;

namespace YurritBotTests;

[TestFixture]
public class BuyCalculatorTests
{
    [TestCase(500, 20, 25)]
    [TestCase(5000, 2, 30)]
    [TestCase(0, 10, 0)]
    public void CalculateMaximuumBuyAmountTest(decimal currentCash, decimal listingPrice, int expectedAmount)
    {
        // Arrange
        var buyCalculator = new BuyCalculator(30);

        // Act
        var maxAmount = buyCalculator.CalculateMaximuumBuyAmount(currentCash, listingPrice);

        // Assert
        maxAmount.Should().Be(expectedAmount);
    }
}