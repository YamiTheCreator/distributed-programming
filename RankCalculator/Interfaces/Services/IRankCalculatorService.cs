namespace RankCalculator.Interfaces.Services;

public interface IRankCalculatorService
{
    Task<double> CalculateRankAsync(string text);
    Task SaveRankAsync(string id, double rank);
    Task ProcessRankCalculationAsync(string id);
}