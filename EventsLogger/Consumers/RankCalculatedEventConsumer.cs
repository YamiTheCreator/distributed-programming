using MassTransit;
using Microsoft.Extensions.Logging;
using ValuatorLib.Contracts;

namespace EventsLogger.Consumers;

public class RankCalculatedEventConsumer(ILogger<RankCalculatedEventConsumer> logger) 
    : IConsumer<RankCalculatedEvent>
{
    public Task Consume(ConsumeContext<RankCalculatedEvent> context)
    {
        RankCalculatedEvent evt = context.Message;
        
        logger.LogInformation(
            "Event: RankCalculated ID: {Id}, Rank: {Rank}", 
            evt.Id, 
            evt.Rank);
        
        return Task.CompletedTask;
    }
}
