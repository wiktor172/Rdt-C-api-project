using System.Net.Http.Json;

namespace WorkExperienceOct2024.Client.Core.Services;

// exactly like StocksService but for the gold endpoint
// the DTO is intentionally a clone of the server record so serialization is painless
public sealed class MarketService(HttpClient http)
{
    public record GoldPriceDto(
        string Pair,
        decimal Price,
        decimal Bid,
        decimal Ask,
        string LastRefreshed,
        long TsUnixMs
    );

    // simple pass-through call to the backend API
    public Task<GoldPriceDto?> GetGoldPriceAsync(CancellationToken ct = default) =>
        http.GetFromJsonAsync<GoldPriceDto>("/api/market/gold/price", ct);
}
