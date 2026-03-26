namespace Valuator.Interfaces.Repositories;

public interface IValuatorRepository
{
    Task SaveTextAsync(string id, string text);
    Task SaveRankAsync(string id, double rank);
    Task SaveSimilarityAsync(string id, double similarity);
    Task<IEnumerable<string>> ListTextsAsync();
    Task<double?> GetRankAsync(string id);
    Task<double?> GetSimilarityAsync(string id);
    Task<bool> TextExistsAsync(string text);
    Task AddTextToSetAsync(string text);
}