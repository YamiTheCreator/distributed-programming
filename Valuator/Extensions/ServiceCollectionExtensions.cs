using MassTransit;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using ValuatorLib.Interfaces;
using ValuatorLib.Models;
using Valuator.Services;
using ValuatorLib.Repositories;
using ValuatorLib.Services;

namespace Valuator.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddValuatorApi(this IServiceCollection services, IConfiguration configuration)
    {
        // Core services
        services.AddScoped<IRankCalculationService, RankCalculationService>();
        services.AddScoped<IValuatorService, ValuatorService>();

        // Main Redis (для Shard Map)
        string mainRedisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        ConnectionMultiplexer mainRedis = ConnectionMultiplexer.Connect(mainRedisConnection);
        services.AddSingleton<IConnectionMultiplexer>(mainRedis);

        // Shard Map Manager
        services.AddScoped<IShardMapManager, ShardMapManager>();

        // Sharded Redis connections
        Dictionary<Region, IConnectionMultiplexer> shardedRedis = new();
        
        string redisRu = configuration.GetConnectionString("RedisRU") ?? "localhost:6380";
        shardedRedis[Region.RU] = ConnectionMultiplexer.Connect(redisRu);
        
        string redisEu = configuration.GetConnectionString("RedisEU") ?? "localhost:6381";
        shardedRedis[Region.EU] = ConnectionMultiplexer.Connect(redisEu);
        
        string redisAsia = configuration.GetConnectionString("RedisASIA") ?? "localhost:6382";
        shardedRedis[Region.ASIA] = ConnectionMultiplexer.Connect(redisAsia);

        services.AddSingleton(shardedRedis);

        // Sharded Repository
        services.AddScoped<IValuatorRepository, ShardedValuatorRepository>();

        // MassTransit configuration
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

        // Data Protection for clustering
        services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(mainRedis, "DataProtection-Keys");

        // Modern API services
        services.AddOpenApi();
        services.AddProblemDetails();
        
        // Health checks
        services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());
    }
}