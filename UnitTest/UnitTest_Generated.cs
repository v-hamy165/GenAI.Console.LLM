using System;
using Xunit;

namespace DemoUnitTest_ConsoleApp.Tests
{
    public class CalculatorTests
    {
        [Fact]
        public void Add_ReturnsSumOfTwoNumbers()
        {
            // Arrange
            var calculator = new Calculator();

            // Act
            int result = calculator.Add(3, 5);

            // Assert
            Assert.Equal(8, result);
        }

        [Fact]
        public void Subtract_ReturnsDifferenceOfTwoNumbers()
        {
            // Arrange
            var calculator = new Calculator();

            // Act
            int result = calculator.Subtract(10, 2);

            // Assert
            Assert.Equal(8, result);
        }
    }
}