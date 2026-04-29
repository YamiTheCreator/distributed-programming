using Microsoft.AspNetCore.Mvc;
using ValuatorLib.Models;
using ValuatorLib.Requests;
using ValuatorLib.Responses;
using Valuator.Services;

namespace Valuator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TextAnalysisController(IValuatorService valuatorService, ILogger<TextAnalysisController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProcessTextResponse>> ProcessText([FromBody] ProcessTextRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Текст обязателен для заполнения");
        }

        if (request.Text.Length > 10000)
        {
            return BadRequest("Текст не может превышать 10000 символов");
        }

        try
        {
            ProcessTextResponse response = await valuatorService.ProcessTextAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке текста");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TextAnalysisResponse>> GetAnalysis(Guid id)
    {
        try
        {
            AnalysisId analysisId = new(id);
            GetAnalysisRequest request = new() { Id = analysisId };
            TextAnalysisResponse response = await valuatorService.GetAnalysisResultAsync(request);

            if (!response.Success)
            {
                return NotFound();
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении анализа для ID: {Id}", id);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
}