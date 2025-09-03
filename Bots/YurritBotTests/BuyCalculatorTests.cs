using FluentAssertions;
using NUnit.Framework;
using YurritBot;

namespace YurritBotTests;

[TestFixture]
public class BuyCalculatorTests
{
    [TestCase(500, 20, 25)]
    [TestCase(5000, 2, 1000)]
    [TestCase(0, 10, 0)]
    public void CalculateMaximuumBuyAmountTest(decimal currentCash, decimal listingPrice, int expectedAmount)
    {
        // Arrange
        var buyer = new BuyCalculator(1000);

        // Act
        var maxAmount = buyer.CalculateMaximuumBuyAmount(currentCash, listingPrice);

        // Assert
        maxAmount.Should().Be(expectedAmount);
    }
}