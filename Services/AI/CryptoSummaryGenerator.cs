using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace LatokenBot.Services.AI;

public class CryptoSummaryGenerator(KernelProvider kernelProvider)
{
    private readonly IChatCompletionService _chatService = kernelProvider.Kernel.GetRequiredService<IChatCompletionService>();

    public async Task<string> GenerateSummaryAsync(
        ParsedRequest parsedRequest, List<(DateTime, decimal)> priceHistory, List<string> newsArticles)
    {
        DateTime today = DateTime.UtcNow.Date;
        string prompt = $$"""
            You are an expert financial assistant specializing in cryptocurrency analysis. Your task is to create a brief summary explaining the reasons behind the price changes of a cryptocurrency.

            Here is the input data:
            - Cryptocurrency Name: {{parsedRequest.CryptoName}}
            - Period: {{today.AddDays(-parsedRequest.PeriodDays):yyyy-MM-dd}} to {{today:yyyy-MM-dd}}
            - Language for the response: {{parsedRequest.Language}}
            - Price History (Date and Price in USD): {{JsonSerializer.Serialize(priceHistory)}}
            - Relevant News Articles: {{string.Join("\n", newsArticles)}}

            Instructions:
            - Analyze the correlation between the news and price changes.
            - If the news provides no clear reason, mention possible generic causes like market trends or trading volume changes.
            - Keep the response concise and written in {{parsedRequest.Language}}.
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
