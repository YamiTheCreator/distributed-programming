using Contracts;
using Valuator.DTO.Requests;
using Valuator.DTO.Responses;
using Valuator.Interfaces.Repositories;
using Valuator.Interfaces.Services;

namespace Valuator.Services;

public class ValuatorService(
    IValuatorRepository repository,
    IRankCalculationService rankCalculationService,
    ILogger<ValuatorService> logger) : IValuatorService
{
    public async Task<ProcessTextResponse> ProcessTextAsync(ProcessTextRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return new ProcessTextResponse
            {
                Success = false,
                ErrorMessage = "Текст не может быть пустым"
            };
        }

        try
        {
            logger.LogDebug("Обработка текста, длина: {Length}", request.Text.Length);

            string id = Guid.NewGuid().ToString();

            await repository.SaveTextAsync(id, request.Text);

            RankCalculationMessage taskMessage = new()
            {
                Id = id
            };
            await rankCalculationService.SendRankCalculationTaskAsync(taskMessage);

            double similarity = await repository.TextExistsAsync(request.Text) ? 1.0 : 0.0;

            if (similarity == 0.0)
            {
                await repository.AddTextToSetAsync(request.Text);
            }

            await repository.SaveSimilarityAsync(id, similarity);

            logger.LogInformation("Успешно обработан текст с ID: {Id}, задание отправлено в очередь", id);

            return new ProcessTextResponse
            {
                Id = id,
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке текста");
            return new ProcessTextResponse
            {
                Success = false,
                ErrorMessage = "Внутренняя ошибка сервера"
            };
        }
    }

    public async Task<TextAnalysisResponse> GetAnalysisResultAsync(GetAnalysisRequest request)
    {
        if (string.IsNullOrEmpty(request.Id))
        {
            return new TextAnalysisResponse
            {
                Success = false,
                ErrorMessage = "ID не может быть пустым"
            };
        }

        try
        {
            logger.LogDebug("Запрос данных для ID: {Id}", request.Id);

            double? rank = await repository.GetRankAsync(request.Id);
            double? similarity = await repository.GetSimilarityAsync(request.Id);

            if (!similarity.HasValue)
            {
                logger.LogWarning("Данные не найдены в Redis для ID: {Id}", request.Id);
                return new TextAnalysisResponse
                {
                    Success = false,
                    ErrorMessage = "Данные не найдены"
                };
            }

            return new TextAnalysisResponse
            {
                Id = request.Id,
                Rank = rank,
                Similarity = similarity.Value,
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Критическая ошибка при загрузке данных для ID: {Id}", request.Id);
            return new TextAnalysisResponse
            {
                Success = false,
                ErrorMessage = "Внутренняя ошибка сервера"
            };
        }
    }
}