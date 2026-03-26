using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RankCalculator.Consumers;
using RankCalculator.Interfaces.Repositories;
using RankCalculator.Interfaces.Services;
using RankCalculator.Repositories;
using RankCalculator.Services;
using StackExchange.Redis;

namespace RankCalculator.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddRankCalculator(this IServiceCollection services, IConfiguration configuration)
    {
        // Настройка Redis
        string redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnection);
        services.AddSingleton<IConnectionMultiplexer>(redis);

        // Регистрация репозитория и сервиса
        services.AddScoped<IRankRepository, RankRepository>();
        services.AddScoped<IRankCalculatorService, RankCalculatorService>();

        // Настройка MassTransit
        services.AddMassTransit(x =>
        {
            x.AddConsumer<RankCalculationConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMQ") ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                    h.RequestedConnectionTimeout(TimeSpan.FromSeconds(30));
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    }
}