namespace RankCalculator.Interfaces.Services;

public interface IRankCalculatorService
{
    Task<double> CalculateRankAsync(string text);
}