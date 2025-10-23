using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace WorkExperienceOct2024.Client.Core.Services;

public sealed class MarketService
{
    private readonly HttpClient _http;
    private readonly ILogger<MarketService> _logger;

    public MarketService(HttpClient http, ILogger<MarketService> logger)
    {
        _http = http;
        _logger = logger;
    }

    // This shape must match what the server returns!
    public record GoldPriceDto(
        string Pair,
        decimal Price,
        decimal Bid,
        decimal Ask,
        string LastRefreshed,
        long TsUnixMs
    );

    public async Task<ApiResult<GoldPriceDto>> GetGoldPriceAsync(CancellationToken ct = default)
    {
        try
        {
            using var response = await _http.GetAsync("/api/market/gold/price", ct);
            if (response.IsSuccessStatusCode)
            {
                var price = await response.Content.ReadFromJsonAsync<GoldPriceDto>(cancellationToken: ct);
                return price is not null
                    ? ApiResult<GoldPriceDto>.Success(price)
                    : ApiResult<GoldPriceDto>.Failure("The server returned an empty response.");
            }

            var error = await ApiClientHelpers.ExtractErrorMessageAsync(response, ct);
            var reason = string.IsNullOrWhiteSpace(response.ReasonPhrase)
                ? response.StatusCode.ToString()
                : response.ReasonPhrase;
            return ApiResult<GoldPriceDto>.Failure(error ?? $"Server returned {(int)response.StatusCode} {reason}.");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NotSupportedException ex)
        {
            _logger.LogError(ex, "Unsupported content when retrieving the gold price");
            return ApiResult<GoldPriceDto>.Failure("The server returned data in an unexpected format.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON when retrieving the gold price");
            return ApiResult<GoldPriceDto>.Failure("We couldn't read the market data returned by the server.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error retrieving gold price");
            return ApiResult<GoldPriceDto>.Failure("We couldn't reach the server. Please try again in a moment.");
        }
    }
}
