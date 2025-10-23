using System.Text.Json;
using WorkExperienceOct2024.Server.Interfaces;
using WorkExperienceOct2024.Server.Models;

namespace WorkExperienceOct2024.Server.Services;

public sealed class AlphaVantageService : IStocksProvider
{
    private const string KEY = "1003M336WQ3A9BW7"; // your key
    private readonly IHttpClientFactory _httpFactory;

    public AlphaVantageService(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    // ---------- QUOTE ----------
    public async Task<QuoteDto> GetQuoteAsync(string symbol, CancellationToken ct = default)
    {
        var http = _httpFactory.CreateClient("alphavantage");
        var url = $"query?function=GLOBAL_QUOTE&symbol={Uri.EscapeDataString(symbol)}&apikey={KEY}";
        using var res = await http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        if (!doc.RootElement.TryGetProperty("Global Quote", out var obj))
            throw new InvalidOperationException("Alpha Vantage response missing 'Global Quote'");

        // helpers
        decimal Dec(string name) =>
            obj.TryGetProperty(name, out var v) && decimal.TryParse(v.GetString(), out var d) ? d : 0m;

        decimal price = Dec("05. price");
        decimal open = Dec("02. open");
        decimal high = Dec("03. high");
        decimal low = Dec("04. low");
        decimal prev = Dec("08. previous close");
        decimal change = Dec("09. change");

        decimal pct = 0m;
        if (obj.TryGetProperty("10. change percent", out var pv))
        {
            var s = pv.GetString()?.Replace("%", "").Trim();
            decimal.TryParse(s, out pct);
        }
        if (change == 0) change = price - prev;
        if (pct == 0 && prev != 0) pct = (price - prev) / prev * 100m;

        var sym = obj.TryGetProperty("01. symbol", out var sv) ? (sv.GetString() ?? symbol) : symbol;

        return new QuoteDto(
            Symbol: sym.ToUpperInvariant(),
            Price: price,
            Open: open,
            High: high,
            Low: low,
            PrevClose: prev,
            Change: change,
            ChangePercent: pct,
            TsUnixMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        );
    }

    // ---------- GOLD (XAU/USD) ----------
    public async Task<GoldPriceDto> GetFxPriceAsync(string from, string to, CancellationToken ct = default)
    {
        var http = _httpFactory.CreateClient("alphavantage");
        var url = $"query?function=CURRENCY_EXCHANGE_RATE&from_currency={Uri.EscapeDataString(from)}&to_currency={Uri.EscapeDataString(to)}&apikey={KEY}";
        using var res = await http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var root = doc.RootElement;

        // ✅ Handle rate limit / errors gracefully
        if (!root.TryGetProperty("Realtime Currency Exchange Rate", out var fx))
        {
            // Alpha Vantage returns Note / Information / Error Message when throttled or bad params
            string msg =
                (root.TryGetProperty("Note", out var note) ? note.GetString() : null) ??
                (root.TryGetProperty("Information", out var info) ? info.GetString() : null) ??
                (root.TryGetProperty("Error Message", out var err) ? err.GetString() : null) ??
                "Unexpected Alpha Vantage response.";
            throw new InvalidOperationException(msg);
        }

        static string? Str(JsonElement o, string name) =>
            o.TryGetProperty(name, out var v) ? v.GetString() : null;

        static decimal Dec(JsonElement o, string name) =>
            o.TryGetProperty(name, out var v) && decimal.TryParse(v.GetString(), out var d) ? d : 0m;

        var pair = $"{from}/{to}";
        var price = Dec(fx, "5. Exchange Rate");
        var bid = Dec(fx, "8. Bid Price");  // may be 0 if AV doesn't send it
        var ask = Dec(fx, "9. Ask Price");  // may be 0 if AV doesn't send it
        var last = Str(fx, "6. Last Refreshed") ?? "";

        return new GoldPriceDto(pair, price, bid, ask, last, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

}

