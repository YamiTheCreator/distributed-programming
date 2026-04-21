using System.Globalization;
using Microsoft.Extensions.Logging;
using ValuatorLib.Interfaces;
using StackExchange.Redis;

namespace ValuatorLib.Repositories;

public class ValuatorRepository(IConnectionMultiplexer redis, ILogger<ValuatorRepository> logger) : IValuatorRepository
{
    private const string TextKeyPrefix = "TEXT-";
    private const string RankKeyPrefix = "RANK-";
    private const string SimilarityKeyPrefix = "SIMILARITY-";
    private const string TextSetKey = "TEXT_SET";
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<string?> GetTextAsync(string id)
    {
        RedisValue value = await _db.StringGetAsync(TextKeyPrefix + id);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task SaveTextAsync(string id, string text)
    {
        await _db.StringSetAsync(TextKeyPrefix + id, text);
        logger.LogDebug("Текст сохранен для ID: {Id}", id);
    }

    public async Task<double?> GetRankAsync(string id)
    {
        RedisValue value = await _db.StringGetAsync(RankKeyPrefix + id);
        return value.HasValue ? double.Parse(value.ToString(), CultureInfo.InvariantCulture) : null;
    }

    public async Task SaveRankAsync(string id, double rank)
    {
        await _db.StringSetAsync(RankKeyPrefix + id, rank.ToString(CultureInfo.InvariantCulture));
        logger.LogInformation("Ранг {Rank} сохранен для ID: {Id}", rank, id);
    }

    public async Task<double?> GetSimilarityAsync(string id)
    {
        RedisValue value = await _db.StringGetAsync(SimilarityKeyPrefix + id);
        return value.HasValue ? double.Parse(value.ToString(), CultureInfo.InvariantCulture) : null;
    }

    public async Task SaveSimilarityAsync(string id, double similarity)
    {
        await _db.StringSetAsync(SimilarityKeyPrefix + id, similarity.ToString(CultureInfo.InvariantCulture));
        logger.LogDebug("Similarity {Similarity} сохранен для ID: {Id}", similarity, id);
    }

    public async Task<bool> TextExistsAsync(string text)
    {
        return await _db.SetContainsAsync(TextSetKey, text);
    }

    public async Task AddTextToSetAsync(string text)
    {
        await _db.SetAddAsync(TextSetKey, text);
    }
}
