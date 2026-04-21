using EventsLogger.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

IHost host = CreateHostBuilder(args).Build();

await host.RunAsync();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<RankCalculatedEventConsumer>();
                x.AddConsumer<SimilarityCalculatedEventConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(hostContext.Configuration.GetConnectionString("RabbitMQ") ?? "localhost", "/", h =>
                    {
                        h.Username(hostContext.Configuration["RabbitMQ:Username"] ?? "guest");
                        h.Password(hostContext.Configuration["RabbitMQ:Password"] ?? "guest");
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });
        });
