using StackExchange.Redis;
using ValuatorLib.Interfaces;
using ValuatorLib.Models;
using Microsoft.Extensions.Logging;

namespace ValuatorLib.Services;

public class ShardMapManager : IShardMapManager
{
    private readonly IConnectionMultiplexer _mainRedis;
    private readonly ILogger<ShardMapManager> _logger;

    public ShardMapManager(IConnectionMultiplexer mainRedis, ILogger<ShardMapManager> logger)
    {
        _mainRedis = mainRedis;
        _logger = logger;
    }

    public async Task SaveShardKeyAsync(AnalysisId id, ShardKey shardKey)
    {
        IDatabase db = _mainRedis.GetDatabase();
        string key = $"shardmap:{id.Value}";
        
        await db.StringSetAsync(key, shardKey.Value);
        
        _logger.LogInformation("Saved shard mapping: {Id} -> {ShardKey}", id.Value, shardKey.Value);
    }

    public async Task<ShardKey?> GetShardKeyAsync(AnalysisId id)
    {
        IDatabase db = _mainRedis.GetDatabase();
        string key = $"shardmap:{id.Value}";
        
        RedisValue value = await db.StringGetAsync(key);
        
        if (value.IsNullOrEmpty)
        {
            _logger.LogWarning("Shard key not found for ID: {Id}", id.Value);
            return null;
        }

        string regionStr = value.ToString();
        if (Enum.TryParse<Region>(regionStr, out Region region))
        {
            _logger.LogInformation("LOOKUP: {Id}, {ShardKey}", id.Value, regionStr);
            return ShardKey.FromRegion(region);
        }

        _logger.LogError("Invalid shard key value: {Value} for ID: {Id}", regionStr, id.Value);
        return null;
    }
}
