using DemoUnitTest_ConsoleApp;
using Xunit;
namespace UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void Add_ShouldReturnCorrectResult()
        {
            Calculator calculator = new Calculator();

            int result = calculator.Add(2, 3);

            Assert.Equal(5, result);
        }
    }
}