using MassTransit;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using ValuatorLib.Interfaces;
using ValuatorLib.Repositories;
using Valuator.Interfaces.Services;
using Valuator.Services;

namespace Valuator.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddValuator(this IServiceCollection services, IConfiguration configuration)
    {
        // Регистрация сервисов
        services.AddScoped<IRankCalculationService, RankCalculationService>();
        services.AddScoped<IValuatorRepository, ValuatorRepository>();
        services.AddScoped<IValuatorService, ValuatorService>();

        // Настройка Redis
        string redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnection);
        services.AddSingleton<IConnectionMultiplexer>(redis);

        // Настройка MassTransit
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMQ") ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        // Настройка Data Protection для работы в кластере
        services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");

        // Add services to the container.
        services.AddRazorPages();
    }
}