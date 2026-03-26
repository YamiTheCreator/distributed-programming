using Microsoft.Extensions.Logging;
using RankCalculator.Interfaces.Repositories;
using RankCalculator.Interfaces.Services;

namespace RankCalculator.Services;

public class RankCalculatorService(IRankRepository rankRepository, ILogger<RankCalculatorService> logger)
    : IRankCalculatorService
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

    public async Task SaveRankAsync(string id, double rank)
    {
        await rankRepository.SaveRankAsync(id, rank);
    }

    public async Task ProcessRankCalculationAsync(string id)
    {
        string? text = await rankRepository.GetTextAsync(id);
        
        if (string.IsNullOrEmpty(text))
        {
            logger.LogWarning("Текст не найден в Redis для ID: {Id}", id);
            throw new InvalidOperationException($"Текст не найден для ID: {id}");
        }

        double rank = await CalculateRankAsync(text);
        await SaveRankAsync(id, rank);
        
        logger.LogInformation("Обработка завершена для ID: {Id}, ранг: {Rank}", id, rank);
    }
}