using WorkExperienceOct2024.Server.Models;

namespace WorkExperienceOct2024.Server.Interfaces;

// this interface keeps the server endpoints decoupled from alpha vantage specifics
// the minimal APIs in Program.cs just ask for "something that gives me quotes"
// which lets us swap out providers later without touching the routing code
public interface IStocksProvider
{
    Task<QuoteDto> GetQuoteAsync(string symbol, CancellationToken ct = default);

    Task<GoldPriceDto> GetFxPriceAsync(string from, string to, CancellationToken ct = default);
}
