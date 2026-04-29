WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

WebApplication app = builder.Build();

// Enable WebSocket support for SignalR
app.UseWebSockets();

app.MapReverseProxy();

app.Run();