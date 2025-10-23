using WorkExperienceOct2024.Server.Models;

namespace WorkExperienceOct2024.Server.Interfaces;

public interface IStocksProvider
{
    Task<QuoteDto> GetQuoteAsync(string symbol, CancellationToken ct = default);

    Task<GoldPriceDto> GetFxPriceAsync(string from, string to, CancellationToken ct = default);
}

