using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RankCalculator.Consumers;
using RankCalculator.Interfaces.Services;
using RankCalculator.Services;
using ValuatorLib.Interfaces;
using ValuatorLib.Models;
using StackExchange.Redis;
using ValuatorLib.Repositories;
using ValuatorLib.Services;

namespace RankCalculator.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddRankCalculator(this IServiceCollection services, IConfiguration configuration)
    {
        // Main Redis (для Shard Map)
        string mainRedisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        ConnectionMultiplexer mainRedis = ConnectionMultiplexer.Connect(mainRedisConnection);
        services.AddSingleton<IConnectionMultiplexer>(mainRedis);

        // Shard Map Manager
        services.AddScoped<IShardMapManager, ShardMapManager>();

        // Sharded Redis connections
        Dictionary<Region, IConnectionMultiplexer> shardedRedis = new Dictionary<Region, IConnectionMultiplexer>();
        
        string redisRu = configuration.GetConnectionString("RedisRU") ?? "localhost:6380";
        shardedRedis[Region.RU] = ConnectionMultiplexer.Connect(redisRu);
        
        string redisEu = configuration.GetConnectionString("RedisEU") ?? "localhost:6381";
        shardedRedis[Region.EU] = ConnectionMultiplexer.Connect(redisEu);
        
        string redisAsia = configuration.GetConnectionString("RedisASIA") ?? "localhost:6382";
        shardedRedis[Region.ASIA] = ConnectionMultiplexer.Connect(redisAsia);

        services.AddSingleton(shardedRedis);

        // Sharded Repository
        services.AddScoped<IValuatorRepository, ShardedValuatorRepository>();
        services.AddScoped<IRankCalculatorService, RankCalculatorService>();

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