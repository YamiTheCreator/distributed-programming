using MassTransit;
using NotificationService.Consumers;

namespace NotificationService.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddNotificationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<RankCalculatedEventConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMQ") ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.ReceiveEndpoint("notification-service-rank-calculated", e =>
                {
                    e.ConfigureConsumer<RankCalculatedEventConsumer>(context);
                });
            });
        });
        
        string redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSignalR()
            .AddStackExchangeRedis(redisConnection, options =>
            {
                options.Configuration.ChannelPrefix = "SignalR";
            });
    }
}