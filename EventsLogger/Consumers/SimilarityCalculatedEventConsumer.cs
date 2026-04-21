using MassTransit;
using Microsoft.Extensions.Logging;
using ValuatorLib.Contracts;

namespace EventsLogger.Consumers;

public class SimilarityCalculatedEventConsumer(ILogger<SimilarityCalculatedEventConsumer> logger) 
    : IConsumer<SimilarityCalculatedEvent>
{
    public Task Consume(ConsumeContext<SimilarityCalculatedEvent> context)
    {
        SimilarityCalculatedEvent evt = context.Message;
        
        logger.LogInformation(
            "Event: SimilarityCalculated ID: {Id}, Similarity: {Similarity}", 
            evt.Id, 
            evt.Similarity);
        
        return Task.CompletedTask;
    }
}
