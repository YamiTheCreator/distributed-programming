using MassTransit;
using Microsoft.AspNetCore.SignalR;
using ValuatorLib.Contracts;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class RankCalculatedEventConsumer(
    IHubContext<NotificationHub> hubContext,
    ILogger<RankCalculatedEventConsumer> logger
) : IConsumer<RankCalculatedEvent>
{
    public async Task Consume(ConsumeContext<RankCalculatedEvent> context)
    {
        RankCalculatedEvent message = context.Message;
        
        logger.LogInformation("Получено событие для ID: {Id}, ранг: {Rank}", message.Id, message.Rank);

        try
        {
            string idString = message.Id.ToString();
            
            await hubContext.Clients.Group($"analysis_{idString}")
                .SendAsync("RankCalculationCompleted", new
                {
                    id = idString,
                    rank = message.Rank
                });

            logger.LogInformation("Отправлено уведомление для ID: {Id}", message.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при отправке уведомления для ID: {Id}", message.Id);
        }
    }
}