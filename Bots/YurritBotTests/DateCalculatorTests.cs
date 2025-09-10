using FluentAssertions;
using NUnit.Framework;
using System;

namespace YurritBot.Tests
{
    [TestFixture]
    public class DateCalculatorTests
    {
        [TestCase("01-01-2025", "01-01-2025", 0)]
        [TestCase("01-01-2025", "01-03-2025", 2)]
        [TestCase("01-01-2025", "01-08-2025", 5)]
        [TestCase("01-01-2025", "01-13-2025", 8)]
        public void CalculateBusinessDaysBetweenTest(DateOnly startDate, DateOnly endDate, int expectedIndex)
        {
            // Arrange
            var dateCalculator = new DateCalculator();

            // Act
            var index = dateCalculator.CalculateBusinessDaysBetween(startDate, endDate);

            // Assert
            index.Should().Be(expectedIndex);
        }
    }
}