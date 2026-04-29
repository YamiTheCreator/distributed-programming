using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Application.Services;
using ValuatorLib.Requests;
using ValuatorLib.Responses;

namespace Application.Pages;

public class IndexModel(ILogger<IndexModel> logger, ITextAnalysisApiService apiService)
    : PageModel
{
    [BindProperty]
    public string? CountryCode { get; set; }
    
    [BindProperty]
    public string? Text { get; set; }

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(CountryCode))
        {
            ErrorMessage = "Выберите страну";
            return Page();
        }
        
        if (string.IsNullOrWhiteSpace(Text))
        {
            ErrorMessage = "Текст не может быть пустым";
            return Page();
        }

        try
        {
            ProcessTextRequest request = new() 
            { 
                Text = Text,
                CountryCode = CountryCode
            };
            ProcessTextResponse? result = await apiService.ProcessTextAsync(request);

            if (result?.Success == true)
            {
                return Redirect($"summary?id={result.Id}");
            }

            ErrorMessage = result?.ErrorMessage ?? "Произошла ошибка при обработке запроса";
            return Page();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при отправке текста на анализ");
            ErrorMessage = "Произошла ошибка при обработке запроса";
            return Page();
        }
    }
}