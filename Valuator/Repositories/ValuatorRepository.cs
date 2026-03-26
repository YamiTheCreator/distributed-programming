using System.Globalization;
using StackExchange.Redis;
using Valuator.Interfaces.Repositories;

namespace Valuator.Repositories;

public class ValuatorRepository(IConnectionMultiplexer redis) : IValuatorRepository
{
    private const string TextKeyPrefix = "TEXT-";
    private const string RankKeyPrefix = "RANK-";
    private const string SimilarityKeyPrefix = "SIMILARITY-";
    private const string TextSetKey = "TEXT_SET";
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task SaveTextAsync(string id, string text)
    {
        await _db.StringSetAsync(TextKeyPrefix + id, text);
    }

    public async Task SaveRankAsync(string id, double rank)
    {
        await _db.StringSetAsync(RankKeyPrefix + id, rank.ToString(CultureInfo.InvariantCulture));
    }

    public async Task SaveSimilarityAsync(string id, double similarity)
    {
        await _db.StringSetAsync(SimilarityKeyPrefix + id, similarity.ToString(CultureInfo.InvariantCulture));
    }

    public async Task<IEnumerable<string>> ListTextsAsync()
    {
        IServer server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
        List<string> texts = [];

        await foreach (RedisKey key in server.KeysAsync(pattern: $"{TextKeyPrefix}*"))
        {
            RedisValue text = await _db.StringGetAsync(key);
            if (text.HasValue)
            {
                texts.Add(text!);
            }
        }

        return texts;
    }

    public async Task<double?> GetRankAsync(string id)
    {
        RedisValue value = await _db.StringGetAsync(RankKeyPrefix + id);
        return value.HasValue ? double.Parse(value.ToString(), CultureInfo.InvariantCulture) : null;
    }

    public async Task<double?> GetSimilarityAsync(string id)
    {
        RedisValue value = await _db.StringGetAsync(SimilarityKeyPrefix + id);
        return value.HasValue ? double.Parse(value.ToString(), CultureInfo.InvariantCulture) : null;
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