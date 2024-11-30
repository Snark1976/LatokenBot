using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace LatokenBot.Services.AI;

public class CryptoSummaryGenerator(Kernel kernel)
{
    private readonly IChatCompletionService _chatService = kernel.GetRequiredService<IChatCompletionService>();

    public async Task<string> GenerateSummaryAsync(
        string cryptoName,
        string language,
        DateTime startDate,
        DateTime endDate,
        List<(DateTime date, decimal price)> priceHistory,
        List<string> newsArticles)
    {
        string prompt = $$"""
            You are an expert financial assistant specializing in cryptocurrency analysis. Your task is to create a brief summary explaining the reasons behind the price changes of a cryptocurrency.

            Here is the input data:
            - Cryptocurrency Name: {{cryptoName}}
            - Period: {{startDate:yyyy-MM-dd}} to {{endDate:yyyy-MM-dd}}
            - Language for the response: {{language}}
            - Price History (Date and Price in USD): {{JsonSerializer.Serialize(priceHistory)}}
            - Relevant News Articles: {{string.Join("\n", newsArticles)}}

            Instructions:
            - Analyze the correlation between the news and price changes.
            - If the news provides no clear reason, mention possible generic causes like market trends or trading volume changes.
            - Keep the response concise and written in {{language}}.
            - Return the response as plain text only, without any additional formatting.
            """;

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(prompt);

        string summary = "";
        await foreach (var message in _chatService.GetStreamingChatMessageContentsAsync(chatHistory))
        {
            summary += message.Content;
        }

        return summary;
    }
}
