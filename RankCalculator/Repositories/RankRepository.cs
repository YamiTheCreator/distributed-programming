using System.Globalization;
using Microsoft.Extensions.Logging;
using RankCalculator.Interfaces.Repositories;
using StackExchange.Redis;

namespace RankCalculator.Repositories;

public class RankRepository(
    IConnectionMultiplexer redis, 
    ILogger<RankRepository> logger) : IRankRepository
{
    private const string RankKeyPrefix = "RANK-";

    private readonly IDatabase _database = redis.GetDatabase();

    public async Task SaveRankAsync(string id, double rank)
    {
        string rankKey = RankKeyPrefix + id;
        string rankValue = rank.ToString(CultureInfo.InvariantCulture);

        await _database.StringSetAsync(rankKey, rankValue);

        logger.LogInformation("Ранг {Rank} сохранен в Redis для ID: {Id}", rank, id);
    }

    public async Task<string?> GetTextAsync(string id)
    {
        string textKey = "TEXT-" + id;
        RedisValue value = await _database.StringGetAsync(textKey);
        return value.HasValue ? value.ToString() : null;
    }
}