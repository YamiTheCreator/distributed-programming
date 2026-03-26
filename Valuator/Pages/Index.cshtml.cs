using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.DTO.Requests;
using Valuator.DTO.Responses;
using Valuator.Interfaces.Services;

namespace Valuator.Pages;

public class IndexModel(ILogger<IndexModel> logger, IValuatorService valuatorService) : PageModel
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
            ProcessTextRequest request = new()
            {
                Text = text
            };
            ProcessTextResponse response = await valuatorService.ProcessTextAsync(request);

            if (!response.Success)
            {
                logger.LogWarning("Ошибка обработки текста: {Error}", response.ErrorMessage);
                return Page();
            }

            return Redirect($"summary?id={response.Id}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Неожиданная ошибка при обработке текста");
            throw;
        }
    }
}