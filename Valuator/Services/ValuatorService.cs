using ValuatorLib.Interfaces;
using ValuatorLib.Contracts;
using MassTransit;
using ValuatorLib.Requests;
using ValuatorLib.Responses;
using ValuatorLib.Models;

namespace Valuator.Services;

public class ValuatorService(
    IValuatorRepository repository,
    IRankCalculationService rankCalculationService,
    IPublishEndpoint publishEndpoint,
    ILogger<ValuatorService> logger) : IValuatorService
{
    public async Task<ProcessTextResponse> ProcessTextAsync(ProcessTextRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return ProcessTextResponse.Failed("Текст не может быть пустым");
        }

        if (string.IsNullOrWhiteSpace(request.CountryCode))
        {
            return ProcessTextResponse.Failed("Страна обязательна для выбора");
        }

        try
        {
            // Определяем страну и регион
            Country? country = Countries.GetByCode(request.CountryCode);
            if (country == null)
            {
                return ProcessTextResponse.Failed($"Неизвестный код страны: {request.CountryCode}");
            }

            ShardKey shardKey = ShardKey.FromCountry(country);
            
            logger.LogDebug("Обработка текста для страны {Country}, регион {Region}, длина: {Length}", 
                country.Name, shardKey.Value, request.Text.Length);

            AnalysisId id = AnalysisId.New();

            // Сохраняем текст в сегмент региона
            await repository.SaveTextAsync(id, request.Text, shardKey);

            RankCalculationMessage taskMessage = new()
            {
                Id = id
            };
            await rankCalculationService.SendRankCalculationTaskAsync(taskMessage);

            // Проверяем схожесть в пределах региона
            double similarity = await repository.TextExistsAsync(request.Text, shardKey) ? 1.0 : 0.0;

            if (similarity == 0.0)
            {
                await repository.AddTextToSetAsync(request.Text, shardKey);
            }

            await repository.SaveSimilarityAsync(id, similarity);

            // публикация события
            await publishEndpoint.Publish(new SimilarityCalculatedEvent
            {
                Id = id,
                Similarity = similarity
            });

            logger.LogInformation("Успешно обработан текст с ID: {Id}, регион: {Region}, задание отправлено в очередь", 
                id, shardKey.Value);

            return ProcessTextResponse.Successful(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке текста");
            return ProcessTextResponse.Failed("Внутренняя ошибка сервера");
        }
    }

    public async Task<TextAnalysisResponse> GetAnalysisResultAsync(GetAnalysisRequest request)
    {
        try
        {
            logger.LogDebug("Запрос данных для ID: {Id}", request.Id);

            double? rank = await repository.GetRankAsync(request.Id);
            double? similarity = await repository.GetSimilarityAsync(request.Id);

            if (!similarity.HasValue)
            {
                logger.LogWarning("Данные не найдены в Redis для ID: {Id}", request.Id);
                return TextAnalysisResponse.Failed("Данные не найдены");
            }

            return TextAnalysisResponse.Successful(request.Id, rank, similarity.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Критическая ошибка при загрузке данных для ID: {Id}", request.Id);
            return TextAnalysisResponse.Failed("Внутренняя ошибка сервера");
        }
    }
}
