using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace WorkExperienceOct2024.Client.Core.Services;

public sealed class StocksService
{
    private readonly HttpClient _http;
    private readonly ILogger<StocksService> _logger;

    public StocksService(HttpClient http, ILogger<StocksService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public record QuoteDto(string Symbol, decimal Price, decimal Open, decimal High,
                           decimal Low, decimal PrevClose, decimal Change,
                           decimal ChangePercent, long TsUnixMs);

    public async Task<ApiResult<QuoteDto>> GetQuoteAsync(string symbol, CancellationToken ct = default)
    {
        try
        {
            using var response = await _http.GetAsync($"/api/stocks/quote/{Uri.EscapeDataString(symbol)}", ct);
            if (response.IsSuccessStatusCode)
            {
                var quote = await response.Content.ReadFromJsonAsync<QuoteDto>(cancellationToken: ct);
                return quote is not null
                    ? ApiResult<QuoteDto>.Success(quote)
                    : ApiResult<QuoteDto>.Failure("The server returned an empty response.");
            }

            var error = await ApiClientHelpers.ExtractErrorMessageAsync(response, ct);
            var reason = string.IsNullOrWhiteSpace(response.ReasonPhrase)
                ? response.StatusCode.ToString()
                : response.ReasonPhrase;
            return ApiResult<QuoteDto>.Failure(error ?? $"Server returned {(int)response.StatusCode} {reason}.");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NotSupportedException ex)
        {
            _logger.LogError(ex, "Unsupported content when fetching quote for symbol {Symbol}", symbol);
            return ApiResult<QuoteDto>.Failure("The server returned data in an unexpected format.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON when fetching quote for symbol {Symbol}", symbol);
            return ApiResult<QuoteDto>.Failure("We couldn't read the quote information returned by the server.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching quote for symbol {Symbol}", symbol);
            return ApiResult<QuoteDto>.Failure("We couldn't reach the server. Please check your connection and try again.");
        }
    }
}
