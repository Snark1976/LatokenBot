using LatokenBot.Services.Redis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace LatokenBot.Services.AI;

public class UserRequestParser(KernelProvider kernelProvider)
{
    private readonly IChatCompletionService _chatService = kernelProvider.Kernel.GetRequiredService<IChatCompletionService>();

    public async Task<ParsedRequest> AnalyzeCryptoQueryAsync(string query, UserContext userContext)
    {
        string prompt = $$"""
            You are an assistant that extracts the name of a cryptocurrency (in English) and the length of a time period in days from any text in any language.
            - Determine the language of the input query.
            - If a clarifying question is needed, return it in the same language as the query.
            - The response should be in JSON format ONLY, without any additional text or symbols.
            - The JSON should have the following structure:

            {
              "CryptoName": string, // The name of the cryptocurrency in English, or "unknown" if not found.
              "PeriodDays": number, // The time period in days, or 0 if not found.
              "ClarifyingQuestion": string, // A clarifying question in the input query"s language, or an empty string if not needed.
              "Language": string // The language of the input query, e.g., English, Russian, etc.
            }

            For example:
            1. Input: "Analyze Bitcoin"
               Output: {
                 "CryptoName": "bitcoin",
                 "PeriodDays": 0,
                 "ClarifyingQuestion": "What is the time period you want to analyze?",
                 "Language": "English"
               }

            2. Input: "Проанализируйте Ethereum за последние 3 недели."
               Output: {
                 "CryptoName": "ethereum",
                 "PeriodDays": 21,
                 "ClarifyingQuestion": "",
                 "Language": "Russian"
               }

            Current context:
            {
                "CryptoName": "{{userContext.ParsedRequest?.CryptoName ?? "unknown"}}",
                "PeriodDays": {{userContext.ParsedRequest?.PeriodDays ?? 0}},
                "Language": "{{userContext.ParsedRequest?.Language ?? "unknown"}}"
            }

            Here is the input query: {{query}}
            """;


        userContext.ChatHistory!.AddSystemMessage(prompt);

        string fullResponse = "";
        await foreach (var message in _chatService.GetStreamingChatMessageContentsAsync(userContext.ChatHistory))
        {
            fullResponse += message.Content;
        }

        try
        {
            string json = ExtractJson(fullResponse);
            return JsonSerializer.Deserialize<ParsedRequest>(json)!;
        }
        catch
        {
            return new ParsedRequest("", -1, "", "en");
        }
    }

    private static string ExtractJson(string response)
    {
        int startIndex = response.IndexOf("{");
        int endIndex = response.LastIndexOf("}");
        if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
        {
            return response.Substring(startIndex, endIndex - startIndex + 1);
        }
        return "{}";
    }
}
