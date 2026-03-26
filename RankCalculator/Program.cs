using Microsoft.Extensions.Hosting;
using RankCalculator.Extensions;

IHost host = CreateHostBuilder(args).Build();

await host.RunAsync();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddRankCalculator(hostContext.Configuration);
        });