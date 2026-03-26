using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.DTO.Requests;
using Valuator.DTO.Responses;
using Valuator.Interfaces.Services;

namespace Valuator.Pages;

public class SummaryModel(ILogger<SummaryModel> logger, IValuatorService valuatorService) : PageModel
{
    public TextAnalysisResponse? AnalysisResult { get; set; }

    public async Task<IActionResult> OnGet(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            logger.LogWarning("Попытка доступа к Summary без Id");
            return RedirectToPage("/Error");
        }

        try
        {
            GetAnalysisRequest request = new() { Id = id };
            TextAnalysisResponse response = await valuatorService.GetAnalysisResultAsync(request);
            
            if (!response.Success)
            {
                logger.LogWarning("Данные не найдены для ID: {Id}, ошибка: {Error}", id, response.ErrorMessage);
                return RedirectToPage("/Error");
            }

            AnalysisResult = response;
            
            return Page();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Критическая ошибка при загрузке Summary для ID: {Id}", id);
            throw;
        }
    }
}