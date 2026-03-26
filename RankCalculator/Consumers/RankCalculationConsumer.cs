using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using RankCalculator.Interfaces.Services;

namespace RankCalculator.Consumers;

public class RankCalculationConsumer(
    IRankCalculatorService rankCalculatorService,
    ILogger<RankCalculationConsumer> logger
) : IConsumer<RankCalculationMessage>
{
    public async Task Consume(ConsumeContext<RankCalculationMessage> context)
    {
        RankCalculationMessage message = context.Message;

        logger.LogInformation("Получено задание на вычисление ранга для ID: {Id}", message.Id);

        try
        {
            if (string.IsNullOrEmpty(message.Id))
            {
                logger.LogWarning("Получено сообщение с пустым ID");
                throw new ArgumentException("ID не может быть пустым");
            }

            await rankCalculatorService.ProcessRankCalculationAsync(message.Id);

            logger.LogInformation("Успешно обработано задание для ID: {Id}", message.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке сообщения для ID: {Id}", message.Id);
            throw; // MassTransit автоматически обработает retry
        }
    }
}