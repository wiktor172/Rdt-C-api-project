using System.Net.Http.Json;

namespace WorkExperienceOct2024.Client.Core.Services;

// this runs inside the browser (WebAssembly) and talks back to our ASP.NET Core host
// it mirrors the QuoteDto contract defined on the server so json can round-trip cleanly
public sealed class StocksService(HttpClient http)
{
    public record QuoteDto(
        string Symbol,
        decimal Price,
        decimal Open,
        decimal High,
        decimal Low,
        decimal PrevClose,
        decimal Change,
        decimal ChangePercent,
        long TsUnixMs
    );

    // the razor page calls this and we just proxy down to /api/stocks/quote/{symbol}
    public Task<QuoteDto?> GetQuoteAsync(string symbol, CancellationToken ct = default) =>
        http.GetFromJsonAsync<QuoteDto>($"/api/stocks/quote/{Uri.EscapeDataString(symbol)}", ct);
}
