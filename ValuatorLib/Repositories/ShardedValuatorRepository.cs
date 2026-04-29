using StackExchange.Redis;
using ValuatorLib.Interfaces;
using ValuatorLib.Models;
using Microsoft.Extensions.Logging;

namespace ValuatorLib.Repositories;

public class ShardedValuatorRepository : IValuatorRepository
{
    private readonly IConnectionMultiplexer _mainRedis;
    private readonly Dictionary<Region, IConnectionMultiplexer> _shardedRedis;
    private readonly IShardMapManager _shardMapManager;
    private readonly ILogger<ShardedValuatorRepository> _logger;

    public ShardedValuatorRepository(
        IConnectionMultiplexer mainRedis,
        Dictionary<Region, IConnectionMultiplexer> shardedRedis,
        IShardMapManager shardMapManager,
        ILogger<ShardedValuatorRepository> logger)
    {
        _mainRedis = mainRedis;
        _shardedRedis = shardedRedis;
        _shardMapManager = shardMapManager;
        _logger = logger;
    }

    private IDatabase GetShardDatabase(ShardKey shardKey)
    {
        if (!_shardedRedis.TryGetValue(shardKey.Region, out IConnectionMultiplexer? redis))
        {
            throw new InvalidOperationException($"Redis connection for region {shardKey.Region} not found");
        }
        return redis.GetDatabase();
    }

    public async Task SaveTextAsync(AnalysisId id, string text, ShardKey shardKey)
    {
        // Сохраняем маппинг в главной БД
        await _shardMapManager.SaveShardKeyAsync(id, shardKey);

        // Сохраняем текст в сегменте
        IDatabase db = GetShardDatabase(shardKey);
        string key = $"text:{id.Value}";
        await db.StringSetAsync(key, text);

        _logger.LogInformation("Saved text to shard {ShardKey}: {Id}", shardKey.Value, id.Value);
    }

    public async Task<string?> GetTextAsync(AnalysisId id)
    {
        // Получаем shard key из главной БД
        ShardKey? shardKey = await _shardMapManager.GetShardKeyAsync(id);
        if (shardKey == null)
        {
            return null;
        }

        // Читаем из сегмента
        IDatabase db = GetShardDatabase(shardKey);
        string key = $"text:{id.Value}";
        RedisValue value = await db.StringGetAsync(key);

        return value.IsNullOrEmpty ? null : value.ToString();
    }

    public async Task SaveRankAsync(AnalysisId id, double rank)
    {
        ShardKey? shardKey = await _shardMapManager.GetShardKeyAsync(id);
        if (shardKey == null)
        {
            throw new InvalidOperationException($"Shard key not found for ID: {id.Value}");
        }

        IDatabase db = GetShardDatabase(shardKey);
        string key = $"rank:{id.Value}";
        await db.StringSetAsync(key, rank);

        _logger.LogInformation("Saved rank to shard {ShardKey}: {Id}", shardKey.Value, id.Value);
    }

    public async Task<double?> GetRankAsync(AnalysisId id)
    {
        ShardKey? shardKey = await _shardMapManager.GetShardKeyAsync(id);
        if (shardKey == null)
        {
            return null;
        }

        IDatabase db = GetShardDatabase(shardKey);
        string key = $"rank:{id.Value}";
        RedisValue value = await db.StringGetAsync(key);

        return value.IsNullOrEmpty ? null : (double?)double.Parse(value.ToString());
    }

    public async Task SaveSimilarityAsync(AnalysisId id, double similarity)
    {
        ShardKey? shardKey = await _shardMapManager.GetShardKeyAsync(id);
        if (shardKey == null)
        {
            throw new InvalidOperationException($"Shard key not found for ID: {id.Value}");
        }

        IDatabase db = GetShardDatabase(shardKey);
        string key = $"similarity:{id.Value}";
        await db.StringSetAsync(key, similarity);

        _logger.LogInformation("Saved similarity to shard {ShardKey}: {Id}", shardKey.Value, id.Value);
    }

    public async Task<double?> GetSimilarityAsync(AnalysisId id)
    {
        ShardKey? shardKey = await _shardMapManager.GetShardKeyAsync(id);
        if (shardKey == null)
        {
            return null;
        }

        IDatabase db = GetShardDatabase(shardKey);
        string key = $"similarity:{id.Value}";
        RedisValue value = await db.StringGetAsync(key);

        return value.IsNullOrEmpty ? null : (double?)double.Parse(value.ToString());
    }

    public async Task<bool> TextExistsAsync(string text, ShardKey shardKey)
    {
        IDatabase db = GetShardDatabase(shardKey);
        string key = $"textset:{shardKey.Value}";
        return await db.SetContainsAsync(key, text);
    }

    public async Task AddTextToSetAsync(string text, ShardKey shardKey)
    {
        IDatabase db = GetShardDatabase(shardKey);
        string key = $"textset:{shardKey.Value}";
        await db.SetAddAsync(key, text);
    }

    public async Task<long> GetUniqueTextsCountAsync(ShardKey shardKey)
    {
        IDatabase db = GetShardDatabase(shardKey);
        string key = $"textset:{shardKey.Value}";
        return await db.SetLengthAsync(key);
    }
}
