using Valuator.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder( args );

builder.Services.AddValuator( builder.Configuration );

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if ( !app.Environment.IsDevelopment() )
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();