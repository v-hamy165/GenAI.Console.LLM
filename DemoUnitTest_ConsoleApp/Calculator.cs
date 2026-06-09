using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

public class Program
{
    static async Task Main()
    {
        // 1. Tìm file Calculator.cs đi ngược lên từ thư mục build (bin)
        string? calculatorPath = FindUpwardFile(AppContext.BaseDirectory, "Calculator.cs");
        if (calculatorPath == null)
        {
            Console.WriteLine("Calculator.cs not found");
            return;
        }
        Console.WriteLine($"Loaded: {calculatorPath}");

        // 2. Đọc toàn bộ nội dung code của file Calculator.cs
        string methodCode = await File.ReadAllTextAsync(calculatorPath, Encoding.UTF8);

        // 3. Xây dựng câu lệnh Prompt gửi cho LLM
        var prompt = $"""
        Write a real xUnit test for the following C# method.
        Do not use Moq or mocking. Just create a real test that calls the method and asserts the result.
        
        Code:
        {methodCode}
        """;

        // 4. Cấu hình HttpClient kết nối tới LM Studio
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(6) }; // Timeout 6 phút đề phòng model nặng
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "lm-studio");

        // 5. Chuẩn bị dữ liệu JSON theo chuẩn OpenAI API
        var body = new
        {
            model = "mistral-7b-instruct-aya-101", // Tên identifier của model trong LM Studio
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 400,
            stream = false,
            temperature = 0.2
        };

        var json = System.Text.Json.JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            Console.WriteLine("Calling Local LLM via LM Studio... Please wait...");
            // 6. Gửi request POST tới endpoint của LM Studio
            var resp = await client.PostAsync("http://localhost:1234/v1/chat/completions", content);
            resp.EnsureSuccessStatusCode(); // Đảm bảo kết nối thành công (Mã 200)

            // 7. Đọc và bóc tách dữ liệu JSON để lấy nội dung text do LLM sinh ra
            var text = await resp.Content.ReadAsStringAsync();
            var raw = JObject.Parse(text)["choices"]![0]!["message"]!["content"]!.ToString();

            // 8. Làm sạch mã nguồn (bỏ các ký hiệu Markdown ``` thừa)
            string unitTestCode = StripCodeFence(raw);

            // 9. Xác định thư mục của UnitTest và lưu file UnitTest_Generated.cs
            var unitTestDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(calculatorPath)!, "..", "UnitTest"));
            Directory.CreateDirectory(unitTestDir);

            string outFile = Path.Combine(unitTestDir, "UnitTest_Generated.cs");
            await File.WriteAllTextAsync(outFile, unitTestCode, Encoding.UTF8);

            Console.WriteLine($"Saved: {outFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during processing: {ex.Message}");
        }
    }

    // Hàm quét ngược lên các thư mục cha để tìm file nguồn
    static string? FindUpwardFile(string start, string name, int max = 8)
    {
        var d = new DirectoryInfo(start);
        for (int i = 0; i < max && d != null; i++, d = d.Parent)
        {
            string c = Path.Combine(d.FullName, name);
            if (File.Exists(c)) return c;
        }
        return null;
    }

    // Hàm cắt bỏ các dấu Markdown ``` bao quanh mã nguồn do AI trả về
    static string StripCodeFence(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;
        int a = s.IndexOf("```");
        if (a >= 0)
        {
            int b = s.IndexOf("```", a + 3);
            if (b > a) s = s.Substring(a + 3, b - a - 3);
            s = s.Replace("csharp", "").Replace("cs", "");
        }
        return s.Trim();
    }
}