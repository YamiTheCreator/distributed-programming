using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redis) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            logger.LogWarning("Попытка отправки пустого текста");
            return Page();
        }

        try
        {
            logger.LogDebug("Обработка текста, длина: {Length}", text.Length);

            string id = Guid.NewGuid().ToString();
            IDatabase db = redis.GetDatabase();

            string textKey = "TEXT-" + id;
            await db.StringSetAsync(textKey, text);

            string rankKey = "RANK-" + id;
            double rank = CalculateRank(text);
            await db.StringSetAsync(rankKey, rank);

            string similarityKey = "SIMILARITY-" + id;
            const string textsSetKey = "TEXT_SET";
            int similarity;

            if (await db.SetContainsAsync(textsSetKey, text))
            {
                similarity = 1;
            }
            else
            {
                await db.SetAddAsync(textsSetKey, text);
                similarity = 0;
            }

            await db.StringSetAsync(similarityKey, similarity);

            logger.LogInformation("Успешно обработан текст с ID: {Id}", id);

            return Redirect($"summary?id={id}");
        }
        catch (RedisConnectionException ex)
        {
            logger.LogError(ex, "Ошибка подключения к Redis при обработке текста");
            return Page();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Неожиданная ошибка при обработке текста");
            throw;
        }
    }

    private static double CalculateRank(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        int alphabeticCount = text.Count(char.IsLetter);
        double nonAlphabeticCount = text.Length - alphabeticCount;

        return text.Length > 0 ? nonAlphabeticCount / text.Length : 0;
    }
}