using Microsoft.Extensions.Logging;
using RankCalculator.Interfaces.Services;

namespace RankCalculator.Services;

public class RankCalculatorService(ILogger<RankCalculatorService> logger) : IRankCalculatorService
{
    public Task<double> CalculateRankAsync(string text)
    {
        if (string.IsNullOrEmpty(text)) 
            return Task.FromResult(0.0);

        int alphabeticCount = text.Count(char.IsLetter);
        int nonAlphabeticCount = text.Length - alphabeticCount;

        double rank = text.Length > 0 ? (double)nonAlphabeticCount / text.Length : 0;
        
        logger.LogDebug("Вычислен ранг {Rank} для текста длиной {Length}", rank, text.Length);
        
        return Task.FromResult(rank);
    }
}
