using StackExchange.Redis;
using System.Text.Json;

namespace LatokenBot.Services.Redis;

public class RedisContextManager
{
    private readonly IDatabase _database;

    public RedisContextManager(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<UserContext> GetOrCreateContextAsync(long userId)
    {
        var key = $"user:{userId}:context";
        string? json = await _database.StringGetAsync(key);

        if (json != null)
        {
            return JsonSerializer.Deserialize<UserContext>(json)!;
        }

        var newContext = new UserContext();
        await SaveContextAsync(userId, newContext);
        return newContext;
    }

    public async Task SaveContextAsync(long userId, UserContext context)
    {
        var key = $"user:{userId}:context";
        string json = JsonSerializer.Serialize(context);
        await _database.StringSetAsync(key, json);
    }
}
