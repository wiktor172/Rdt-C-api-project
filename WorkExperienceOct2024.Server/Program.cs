using WorkExperienceOct2024.Server.Interfaces;
using WorkExperienceOct2024.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("alphavantage", c =>
{
    c.BaseAddress = new Uri("https://www.alphavantage.co/");
});
builder.Services.AddScoped<IStocksProvider, AlphaVantageService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// --- endpoints (ONE of each) ---
app.MapGet("/api/stocks/quote/{symbol}",
    async (string symbol, IStocksProvider svc, CancellationToken ct)
        => Results.Ok(await svc.GetQuoteAsync(symbol, ct)));

app.MapGet("/api/market/gold/price",
    async (IStocksProvider svc, CancellationToken ct)
        => Results.Ok(await svc.GetFxPriceAsync("XAU", "USD", ct)));

// --- SPA fallback LAST ---
app.MapFallbackToFile("index.html");

app.Run();

