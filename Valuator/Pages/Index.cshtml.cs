using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redis) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string text)
    {
        logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();
        IDatabase db = redis.GetDatabase();

        string textKey = "TEXT-" + id;
        await db.StringSetAsync(textKey, text);

        string rankKey = "RANK-" + id;
        double rank = CalculateRank(text);
        await db.StringSetAsync(rankKey, rank);

        string similarityKey = "SIMILARITY-" + id;
        string textsSetKey = "TEXT_SET";
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

        db.StringSet(similarityKey, similarity);

        return Redirect($"summary?id={id}");
    }

    private double CalculateRank(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        int alphabeticCount = text.Count(char.IsLetter);

        double nonAlphabeticCount = text.Length - alphabeticCount;
        return nonAlphabeticCount / text.Length;
    }
}