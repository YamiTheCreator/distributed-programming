namespace ValuatorLib.Interfaces;

public interface IValuatorRepository
{
    Task<string?> GetTextAsync(string id);
    Task SaveTextAsync(string id, string text);
    Task<double?> GetRankAsync(string id);
    Task SaveRankAsync(string id, double rank);
    Task<double?> GetSimilarityAsync(string id);
    Task SaveSimilarityAsync(string id, double similarity);
    Task<bool> TextExistsAsync(string text);
    Task AddTextToSetAsync(string text);
}
