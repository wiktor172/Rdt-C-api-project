using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkExperienceOct2024.Server.Interfaces;
using WorkExperienceOct2024.Server.Models;

namespace WorkExperienceOct2024.Server.Controllers;

[ApiController]
[Route("api/market")]
public sealed class MarketController : ControllerBase
{
    private readonly IStocksProvider _stocksProvider;
    private readonly ILogger<MarketController> _logger;

    public MarketController(IStocksProvider stocksProvider, ILogger<MarketController> logger)
    {
        _stocksProvider = stocksProvider;
        _logger = logger;
    }

    [HttpGet("gold/price")]
    [ProducesResponseType(typeof(GoldPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetGoldPrice(CancellationToken cancellationToken)
    {
        try
        {
            var price = await _stocksProvider.GetFxPriceAsync("XAU", "USD", cancellationToken);
            return Ok(price);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Unable to retrieve gold price from Alpha Vantage");
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable, title: "Alpha Vantage unavailable");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while retrieving gold price");
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable, title: "Network error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving gold price");
            return Problem(detail: "An unexpected error occurred while retrieving the gold price.", statusCode: StatusCodes.Status500InternalServerError, title: "Unexpected error");
        }
    }
}
