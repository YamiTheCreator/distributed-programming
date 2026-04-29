using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Application.Services;
using ValuatorLib.Responses;

namespace Application.Pages;

public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly ITextAnalysisApiService _apiService;

    public SummaryModel(ILogger<SummaryModel> logger, ITextAnalysisApiService apiService)
    {
        _logger = logger;
        _apiService = apiService;
    }

    public TextAnalysisResponse? AnalysisResult { get; set; }

    public async Task<IActionResult> OnGet(string id)
    {
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid guidId))
        {
            _logger.LogWarning("Попытка доступа к Summary с некорректным Id: {Id}", id);
            return RedirectToPage("/Error");
        }

        try
        {
            AnalysisResult = await _apiService.GetAnalysisAsync(guidId);

            if (AnalysisResult?.Success == true)
            {
                return Page();
            }

            _logger.LogWarning("Данные не найдены для ID: {Id}", id);
            return RedirectToPage("/Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критическая ошибка при загрузке Summary для ID: {Id}", id);
            return RedirectToPage("/Error");
        }
    }
}