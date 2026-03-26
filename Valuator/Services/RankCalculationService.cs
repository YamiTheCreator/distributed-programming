using Contracts;
using MassTransit;
using Valuator.Interfaces.Services;

namespace Valuator.Services;

public class RankCalculationService(
    IPublishEndpoint publishEndpoint,
    ILogger<RankCalculationService> logger) : IRankCalculationService
{
    public async Task SendRankCalculationTaskAsync(RankCalculationMessage message)
    {
        logger.LogInformation("Отправка задания на вычисление ранга для ID: {Id}", message.Id);

        await publishEndpoint.Publish(message);

        logger.LogDebug("Сообщение успешно отправлено в MassTransit");
    }
}