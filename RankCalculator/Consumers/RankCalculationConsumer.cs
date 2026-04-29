using ValuatorLib.Contracts;
using ValuatorLib.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using RankCalculator.Interfaces.Services;

namespace RankCalculator.Consumers;

public class RankCalculationConsumer(
    IRankCalculatorService rankCalculatorService,
    IValuatorRepository valuatorRepository,
    ILogger<RankCalculationConsumer> logger
) : IConsumer<RankCalculationMessage>
{
    public async Task Consume(ConsumeContext<RankCalculationMessage> context)
    {
        RankCalculationMessage message = context.Message;

        logger.LogInformation("Получено задание на вычисление ранга для ID: {Id}", message.Id);

        try
        {
            if (message.Id.Value == Guid.Empty)
            {
                logger.LogWarning("Получено некорректное сообщение");
                throw new ArgumentException("Некорректные данные в сообщении");
            }

            string? text = await valuatorRepository.GetTextAsync(message.Id);
            if (string.IsNullOrEmpty(text))
            {
                logger.LogWarning("Текст не найден в Redis для ID: {Id}", message.Id);
                throw new InvalidOperationException($"Текст не найден для ID: {message.Id}");
            }

            // Добавляем случайную задержку от 3 до 15 секунд
            TimeSpan interval = TimeSpan.FromSeconds(new Random().Next(3, 15));
            logger.LogInformation("Waiting {Interval} for ID: {Id}", interval, message.Id);
            await Task.Delay(interval, context.CancellationToken);

            double rank = await rankCalculatorService.CalculateRankAsync(text);
            await valuatorRepository.SaveRankAsync(message.Id, rank);

            await context.Publish(new RankCalculatedEvent
            {
                Id = message.Id,
                Rank = rank
            });

            logger.LogInformation("Успешно обработано задание для ID: {Id}, ранг: {Rank}", message.Id, rank);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке сообщения для ID: {Id}", message.Id);
            throw;
        }
    }
}