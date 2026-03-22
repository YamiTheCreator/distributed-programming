using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class SummaryModel(ILogger<SummaryModel> logger, IConnectionMultiplexer redis) : PageModel
{
    public double Rank { get; set; }
    public double Similarity { get; set; }

    public async Task<IActionResult> OnGet(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            logger.LogWarning("Попытка доступа к Summary без Id");
            return RedirectToPage("/Error");
        }

        try
        {
            logger.LogDebug("Запрос данных для ID: {Id}", id);

            IDatabase db = redis.GetDatabase();

            RedisValue rankValue = await db.StringGetAsync("RANK-" + id);
            RedisValue similarityValue = await db.StringGetAsync("SIMILARITY-" + id);

            if (rankValue.IsNullOrEmpty || similarityValue.IsNullOrEmpty)
            {
                logger.LogWarning("Данные не найдены в Redis для ID: {Id}", id);
                return RedirectToPage("/Error");
            }

            Rank = (double)rankValue;
            Similarity = (double)similarityValue;

            return Page();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Критическая ошибка при загрузке Summary для ID: {Id}", id);

            throw;
        }
    }
}