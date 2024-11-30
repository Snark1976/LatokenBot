using LatokenBot.Services.AI;
using LatokenBot.Services.PlottingGraphs;
using LatokenBot.Services.ReceivingData;
using LatokenBot.Services.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LatokenBot.Services.Telegram.Handlers;
public class MessageHandler(
    ITelegramBotClient botClient, 
    RedisContextManager contextManager,
    UserRequestParser userRequestParser,
    CryptoSummaryGenerator summaryGenerator,
    IChartBuilder chartBuilder,
    INewsFetcher newsFetcher )
{
    public async Task HandleMessageAsync(Message message)
    {
        if (message?.Text is not { } messageText) return;

        long chatId = message.Chat.Id;

        UserContext userContext = await contextManager.GetOrCreateContextAsync(chatId);

        if (messageText == "/start")
        {
            await ClearContextAsync(userContext, chatId);
            await botClient.SendMessage(chatId, "Welcome to CryptoBot!");
            return;
        }

        if (userContext.ParsedRequest is null || !string.IsNullOrEmpty(userContext.ParsedRequest.ClarifyingQuestion))
        {
            userContext.ChatHistory ??= [];
            userContext.ChatHistory.AddUserMessage(messageText);
            userContext.ParsedRequest = await userRequestParser
                .AnalyzeCryptoQueryAsync(messageText, userContext);
            if (!string.IsNullOrEmpty(userContext.ParsedRequest.ClarifyingQuestion))
            {
                await contextManager.SaveContextAsync(chatId, userContext);
                await botClient.SendMessage(chatId, userContext.ParsedRequest.ClarifyingQuestion);
                return;
            }
        }

        await PrepareSummaryAsync(userContext.ParsedRequest, chatId);
        await ClearContextAsync(userContext, chatId);
    }

    private async Task PrepareSummaryAsync(ParsedRequest parsedRequest, long chatId)
    {
        List<(DateTime date, decimal price)> priceHistory = await CryptoPriceTracker.GetPriceHistoryAsync(parsedRequest);
        List<string> newsArticles;
        try
        {
            newsArticles = await newsFetcher.FetchNewsAsync(parsedRequest);
        }
        catch (InvalidOperationException ex)
        {
            await botClient.SendMessage(chatId, ex.Message);
            return;
        }
        string summary = await summaryGenerator.GenerateSummaryAsync(parsedRequest, priceHistory, newsArticles);
        InputFile plot = chartBuilder.PlotPriceHistory(priceHistory, parsedRequest.CryptoName);
        await botClient.SendPhoto(chatId, plot);
        await botClient.SendMessage(chatId, summary);
    }

    private async Task ClearContextAsync(UserContext userContext, long chatId)
    {
        userContext.ParsedRequest = null;
        userContext.ChatHistory = null;
        await contextManager.SaveContextAsync(chatId, userContext);
    }
}
