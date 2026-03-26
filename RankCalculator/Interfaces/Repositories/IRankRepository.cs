namespace RankCalculator.Interfaces.Repositories;

public interface IRankRepository
{
    Task SaveRankAsync(string id, double rank);
    Task<string?> GetTextAsync(string id);
}