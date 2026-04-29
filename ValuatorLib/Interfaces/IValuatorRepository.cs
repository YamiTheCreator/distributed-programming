using ValuatorLib.Models;

namespace ValuatorLib.Interfaces;

public interface IValuatorRepository
{
    Task<string?> GetTextAsync(AnalysisId id);
    Task SaveTextAsync(AnalysisId id, string text, ShardKey shardKey);
    Task<double?> GetRankAsync(AnalysisId id);
    Task SaveRankAsync(AnalysisId id, double rank);
    Task<double?> GetSimilarityAsync(AnalysisId id);
    Task SaveSimilarityAsync(AnalysisId id, double similarity);
    Task<bool> TextExistsAsync(string text, ShardKey shardKey);
    Task AddTextToSetAsync(string text, ShardKey shardKey);
    Task<long> GetUniqueTextsCountAsync(ShardKey shardKey);
}
