using ValuatorLib.Models;

namespace ValuatorLib.Interfaces;

public interface IShardMapManager
{
    Task SaveShardKeyAsync(AnalysisId id, ShardKey shardKey);
    Task<ShardKey?> GetShardKeyAsync(AnalysisId id);
}
