using WorkExperienceOct2024.Server.Interfaces;
using WorkExperienceOct2024.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// DI wiring: register a named HttpClient that knows how to talk to Alpha Vantage
builder.Services.AddHttpClient("alphavantage", c =>
{
    c.BaseAddress = new Uri("https://www.alphavantage.co/");
});

// when the minimal APIs ask for IStocksProvider the container gives them AlphaVantageService
builder.Services.AddScoped<IStocksProvider, AlphaVantageService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// --- endpoints (ONE of each) ---
// this route feeds the StocksService on the wasm side; it returns QuoteDto json
app.MapGet("/api/stocks/quote/{symbol}",
    async (string symbol, IStocksProvider svc, CancellationToken ct)
        => Results.Ok(await svc.GetQuoteAsync(symbol, ct)));

// this one feeds the MarketService in the wasm client; it returns GoldPriceDto json
app.MapGet("/api/market/gold/price",
    async (IStocksProvider svc, CancellationToken ct)
        => Results.Ok(await svc.GetFxPriceAsync("XAU", "USD", ct)));

// --- SPA fallback LAST ---
app.MapFallbackToFile("index.html");

app.Run();
