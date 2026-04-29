using Application.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

string apiGatewayUrl = builder.Configuration["ApiGateway:Url"] ?? "http://localhost:8080";
builder.Services.AddHttpClient<ITextAnalysisApiService, TextAnalysisApiService>(client =>
{
    client.BaseAddress = new Uri(apiGatewayUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();