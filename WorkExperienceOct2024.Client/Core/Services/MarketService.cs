namespace WorkExperienceOct2024.Client.Core.Services;

using System.Net.Http.Json;

public sealed class MarketService(HttpClient http)
{
    // This shape must match what the server returns!
    public record GoldPriceDto(
        string Pair,
        decimal Price,
        decimal Bid,
        decimal Ask,
        string LastRefreshed,
        long TsUnixMs
    );

    public Task<GoldPriceDto?> GetGoldPriceAsync(CancellationToken ct = default)
        => http.GetFromJsonAsync<GoldPriceDto>("/api/market/gold/price", ct);
}

