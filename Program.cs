using DotNetEnv;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LatokenBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Загрузка секретов из .env
        Env.Load(); 

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            "gpt-4-1106-preview",
            Environment.GetEnvironmentVariable("openai-api-key")!);
        var kernel = builder.Build();

        RequestParser requestParser = new(kernel);
        string queryForUser = "Enter you query";
        var chatHistory = new ChatHistory();
        string query = string.Empty;
        ParsedRequest parsedRequest = null!;
        while (query != "Exit")
        {
            Console.Write($"Assistant > {queryForUser}\nUser >");
            query = Console.ReadLine()!;
            parsedRequest = await requestParser.AnalyzeCryptoQueryAsync(query, chatHistory);
            if (string.IsNullOrEmpty(parsedRequest.ClarifyingQuestion)) break;
            queryForUser = parsedRequest.ClarifyingQuestion;
        }
        Console.WriteLine(parsedRequest);
        List<string> newsArticles = await NewsFetcher.FetchNewsAsync(parsedRequest.CryptoName, parsedRequest.PeriodDays);
        DateTime today = DateTime.UtcNow.Date;
        List<(DateTime date, decimal price)> priceHistory =
            await CryptoPriceTracker.GetPriceHistoryAsync(parsedRequest.CryptoName, "usd", today.AddDays(-parsedRequest.PeriodDays), today);
        CryptoSummaryGenerator summaryGenerator = new(kernel);
        string summary = await summaryGenerator.GenerateSummaryAsync(
            cryptoName: parsedRequest.CryptoName,
            language: parsedRequest.Language,
            startDate: today.AddDays(-parsedRequest.PeriodDays),
            endDate: today,
            priceHistory: priceHistory,
            newsArticles: newsArticles
            );
        Console.WriteLine(summary);
        ChartBuilder.PlotPriceHistory(priceHistory);
        Console.ReadLine();
    }
}
