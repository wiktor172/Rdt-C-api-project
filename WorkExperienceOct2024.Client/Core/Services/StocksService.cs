using System.Net.Http.Json;

namespace WorkExperienceOct2024.Client.Core.Services;

public sealed class StocksService(HttpClient http)
{
    public record QuoteDto(string Symbol, decimal Price, decimal Open, decimal High,
                           decimal Low, decimal PrevClose, decimal Change,
                           decimal ChangePercent, long TsUnixMs);

    public Task<QuoteDto?> GetQuoteAsync(string symbol, CancellationToken ct = default) =>
        http.GetFromJsonAsync<QuoteDto>($"/api/stocks/quote/{Uri.EscapeDataString(symbol)}", ct);
}

