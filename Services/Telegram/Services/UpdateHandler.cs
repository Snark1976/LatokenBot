using LatokenBot.Services.Telegram.Handlers;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace LatokenBot.Services.Telegram.Services;

public class UpdateHandler(ILogger<UpdateHandler> logger, MessageHandler messageHandler) : IUpdateHandler
{
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogInformation("HandleError: {Exception}", exception);
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await (update switch
        {
            { Message: { } message } => messageHandler.HandleMessageAsync(message),
            _                        => UnknownUpdateHandlerAsync(update)
        });
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        logger.LogInformation("Unhandled update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}
