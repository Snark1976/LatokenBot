using LatokenBot.Services.AI;
using LatokenBot.Services.PlottingGraphs;
using LatokenBot.Services.ReceivingData;
using LatokenBot.Services.Redis;
using LatokenBot.Services.Telegram;
using LatokenBot.Services.Telegram.Handlers;
using LatokenBot.Services.Telegram.Services;
using Microsoft.Extensions.Options;
using NewsAPI;
using StackExchange.Redis;
using Telegram.Bot;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<BotConfiguration>(context.Configuration.GetSection("BotConfiguration"));
        services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    BotConfiguration? botConfiguration = sp.GetService<IOptions<BotConfiguration>>()?.Value;
                    ArgumentNullException.ThrowIfNull(botConfiguration);
                    TelegramBotClientOptions options = new(botConfiguration.BotToken);
                    return new TelegramBotClient(options, httpClient);
                });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(context.Configuration.GetSection("Redis").GetValue<string>("ConnectionString")!));

        services.Configure<NewsApiSettings>(context.Configuration.GetSection("NewsApi"));
        services.AddScoped<NewsApiClient>(sp =>
        {
            var apiKey = sp.GetRequiredService<IOptions<NewsApiSettings>>().Value.ApiKey;
            return new NewsApiClient(apiKey);
        });

        services
            .AddSingleton<RedisContextManager>()
            .AddSingleton<KernelProvider>();

        services
            .AddScoped<UpdateHandler>()
            .AddScoped<ReceiverService>()
            .AddScoped<UserRequestParser>()
            .AddScoped<CryptoSummaryGenerator>()
            .AddScoped<MessageHandler>()
            .AddScoped<INewsFetcher, NewsFetcher>();


        services
            .AddTransient<IChartBuilder, ChartBuilder>();

        services
            .AddHostedService<PollingService>();
    })
    .Build();

await host.RunAsync();