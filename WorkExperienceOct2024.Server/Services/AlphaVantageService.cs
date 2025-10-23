using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WorkExperienceOct2024.Server.Interfaces;
using WorkExperienceOct2024.Server.Models;
using WorkExperienceOct2024.Server.Options;

namespace WorkExperienceOct2024.Server.Services;

public sealed class AlphaVantageService : IStocksProvider
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly AlphaVantageOptions _options;
    private readonly ILogger<AlphaVantageService> _logger;

    public AlphaVantageService(
        IHttpClientFactory httpFactory,
        IOptions<AlphaVantageOptions> options,
        ILogger<AlphaVantageService> logger)
    {
        _httpFactory = httpFactory;
        _options = options.Value;
        _logger = logger;
    }

    // ---------- QUOTE ----------
    public async Task<QuoteDto> GetQuoteAsync(string symbol, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));
        }

        var http = _httpFactory.CreateClient("alphavantage");
        var url = $"query?function=GLOBAL_QUOTE&symbol={Uri.EscapeDataString(symbol)}&apikey={GetApiKey()}";
        using var res = await http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        var root = doc.RootElement;
        ThrowIfAlphaVantageReturnedError(root);

        if (!root.TryGetProperty("Global Quote", out var obj) || IsEmptyObject(obj))
        {
            throw new InvalidOperationException($"No quote data returned for symbol '{symbol}'.");
        }

        // helpers
        decimal Dec(string name) => ParseDecimal(obj, name);

        decimal price = Dec("05. price");
        decimal open = Dec("02. open");
        decimal high = Dec("03. high");
        decimal low = Dec("04. low");
        decimal prev = Dec("08. previous close");
        decimal change = Dec("09. change");

        var pct = ParsePercentage(obj, "10. change percent");
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
        if (string.IsNullOrWhiteSpace(from))
        {
            throw new ArgumentException("Source currency cannot be empty.", nameof(from));
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            throw new ArgumentException("Target currency cannot be empty.", nameof(to));
        }

        var http = _httpFactory.CreateClient("alphavantage");
        var url = $"query?function=CURRENCY_EXCHANGE_RATE&from_currency={Uri.EscapeDataString(from)}&to_currency={Uri.EscapeDataString(to)}&apikey={GetApiKey()}";
        using var res = await http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var root = doc.RootElement;

        ThrowIfAlphaVantageReturnedError(root);

        if (!root.TryGetProperty("Realtime Currency Exchange Rate", out var fx) || IsEmptyObject(fx))
        {
            throw new InvalidOperationException("Alpha Vantage response did not include exchange rate data.");
        }

        static string? Str(JsonElement o, string name) =>
            o.TryGetProperty(name, out var v) ? v.GetString() : null;

        static decimal Dec(JsonElement o, string name) => ParseDecimal(o, name);

        var pair = $"{from}/{to}";
        var price = Dec(fx, "5. Exchange Rate");
        var bid = Dec(fx, "8. Bid Price");  // may be 0 if AV doesn't send it
        var ask = Dec(fx, "9. Ask Price");  // may be 0 if AV doesn't send it
        var last = Str(fx, "6. Last Refreshed") ?? "";

        return new GoldPriceDto(pair, price, bid, ask, last, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    private static decimal ParseDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return 0m;
        }

        var raw = value.GetString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return 0m;
        }

        return decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0m;
    }

    private static decimal ParsePercentage(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return 0m;
        }

        var raw = value.GetString()?.Replace("%", string.Empty, StringComparison.Ordinal).Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return 0m;
        }

        return decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0m;
    }

    private static bool IsEmptyObject(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var _ in element.EnumerateObject())
        {
            return false;
        }

        return true;
    }

    private static void ThrowIfAlphaVantageReturnedError(JsonElement root)
    {
        string? message = null;

        if (root.TryGetProperty("Note", out var note))
        {
            message = note.GetString();
        }
        else if (root.TryGetProperty("Information", out var info))
        {
            message = info.GetString();
        }
        else if (root.TryGetProperty("Error Message", out var error))
        {
            message = error.GetString();
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            throw new InvalidOperationException(message);
        }
    }

    private string GetApiKey()
    {
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return _options.ApiKey;
        }

        var envKey = Environment.GetEnvironmentVariable("ALPHAVANTAGE_API_KEY");
        if (!string.IsNullOrWhiteSpace(envKey))
        {
            return envKey;
        }

        const string message = "Alpha Vantage API key has not been configured. Set AlphaVantage:ApiKey in configuration or the ALPHAVANTAGE_API_KEY environment variable.";
        _logger.LogError(message);
        throw new InvalidOperationException(message);
    }
}

