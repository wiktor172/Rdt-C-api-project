using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkExperienceOct2024.Server.Interfaces;
using WorkExperienceOct2024.Server.Models;

namespace WorkExperienceOct2024.Server.Controllers;

[ApiController]
[Route("api/stocks")]
public sealed class StocksController : ControllerBase
{
    private readonly IStocksProvider _stocksProvider;
    private readonly ILogger<StocksController> _logger;

    public StocksController(IStocksProvider stocksProvider, ILogger<StocksController> logger)
    {
        _stocksProvider = stocksProvider;
        _logger = logger;
    }

    [HttpGet("quote/{symbol}")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetQuote(string symbol, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return Problem(
                detail: "A stock symbol is required.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Missing symbol");
        }

        try
        {
            var quote = await _stocksProvider.GetQuoteAsync(symbol, cancellationToken);
            return Ok(quote);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Unable to retrieve quote for symbol {Symbol}", symbol);

            return ex.Message.Contains("No quote data", StringComparison.OrdinalIgnoreCase)
                ? Problem(detail: ex.Message, statusCode: StatusCodes.Status404NotFound, title: "Quote not found")
                : Problem(detail: ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable, title: "Alpha Vantage unavailable");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while retrieving quote for symbol {Symbol}", symbol);
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable, title: "Network error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving quote for symbol {Symbol}", symbol);
            return Problem(detail: "An unexpected error occurred while retrieving the quote.", statusCode: StatusCodes.Status500InternalServerError, title: "Unexpected error");
        }
    }
}
