using System.Text.Json;
using WorkExperienceOct2024.Server.Interfaces;
using WorkExperienceOct2024.Server.Models;

namespace WorkExperienceOct2024.Server.Services;

// this class is the glue between our ASP.NET Core backend and the Alpha Vantage HTTP api
// Program.cs only knows about the IStocksProvider abstraction, and dependency injection
// hands this implementation in whenever a request hits the /api endpoints
public sealed class AlphaVantageService : IStocksProvider
{
    // keeping the key here for now so the sample runs, but normally you'd read from config
    private const string KEY = "1003M336WQ3A9BW7";
    private readonly IHttpClientFactory _httpFactory;

    public AlphaVantageService(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    // ---------- QUOTE ----------
    // when the front-end asks for /api/stocks/quote/{symbol} the minimal api routes call this
    // we call Alpha Vantage, massage the json into our QuoteDto shape, and send it back up the stack
    public async Task<QuoteDto> GetQuoteAsync(string symbol, CancellationToken ct = default)
    {
        var http = _httpFactory.CreateClient("alphavantage");
        var url = $"query?function=GLOBAL_QUOTE&symbol={Uri.EscapeDataString(symbol)}&apikey={KEY}";
        using var res = await http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        if (!doc.RootElement.TryGetProperty("Global Quote", out var obj))
        {
            // if AV doesn't send the property we expect, we fail fast so the UI can show an error
            throw new InvalidOperationException("Alpha Vantage response missing 'Global Quote'");
        }

        // helpers to keep the parsing noise tidy and easy to read later
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
    // similar story for the gold endpoint: the minimal api route delegates to here
    public async Task<GoldPriceDto> GetFxPriceAsync(string from, string to, CancellationToken ct = default)
    {
        var http = _httpFactory.CreateClient("alphavantage");
        var url = $"query?function=CURRENCY_EXCHANGE_RATE&from_currency={Uri.EscapeDataString(from)}&to_currency={Uri.EscapeDataString(to)}&apikey={KEY}";
        using var res = await http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var root = doc.RootElement;

        // Alpha Vantage can throttle or error by returning Note / Information / Error Message blocks
        if (!root.TryGetProperty("Realtime Currency Exchange Rate", out var fx))
        {
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
        var bid = Dec(fx, "8. Bid Price");  // these can legitimately be zero depending on the feed
        var ask = Dec(fx, "9. Ask Price");
        var last = Str(fx, "6. Last Refreshed") ?? "";

        return new GoldPriceDto(pair, price, bid, ask, last, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }
}
