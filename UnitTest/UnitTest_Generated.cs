Code test xUnit cho phương thức này có thể được viết như sau:

```
using System;
using Xunit;
using static System.IO.Path;

namespace UnitTest_Generated
{
    public class TestCalculator
    {
        [Fact]
        public void Test()
        {
            // 1. Tìm file Calculator. đi ngược lên từ thư mục build (bin)
            string? calculatorPath = FindUpwardFile(AppContext.BaseDirectory, "Calculator.");
            if (calculatorPath == null)
            {
                Assert.Throws<Exception>(() => Program.MainAsync().Wait());
                return;
            }
            Console.WriteLine($"Loaded: {calculatorPath}");

            // 2. Đọc toàn bộ nội dung code của file Calculator.
            string methodCode = File.ReadAllText(calculatorPath, Encoding.UTF8);

            // 3. Xây dựng câu lệnh Prompt gửi cho LLM
            var prompt = $"""
            Write a real xUnit test for the following C# method.\nDo not use Moq or mocking. Just create a real test that calls the method and asserts the result.\n\nCode:\n{methodCode}\n""";

            // 4. Tạo một đối tượng Program và gọi phương thức MainAsync() của nó
            var program = new Program