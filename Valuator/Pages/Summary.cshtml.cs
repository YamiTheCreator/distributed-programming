using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Valuator.Pages;

public class SummaryModel(ILogger<SummaryModel> logger, IConnectionMultiplexer redis) : PageModel
{
    public double Rank { get; set; }
    public double Similarity { get; set; }

    public async Task<IActionResult> OnGet(string id)
    {
        logger.LogDebug(id);

        IDatabase db = redis.GetDatabase();

        Rank = (double)await db.StringGetAsync("RANK-" + id);
        Similarity = (double)await db.StringGetAsync("SIMILARITY-" + id);

        return Page();
    }
}