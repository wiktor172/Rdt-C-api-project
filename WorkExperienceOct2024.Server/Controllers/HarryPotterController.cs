using Microsoft.AspNetCore.Mvc;
using WorkExperienceOct2024.Server.Interfaces;

namespace WorkExperienceOct2024.Server.Controllers
{
    [ApiController]
    [Route("harrypotter/")]
    public class HarryPotterController : ControllerBase
    {
        private readonly ILogger<HarryPotterController> _logger;

        private IStocksProvider harryPotterService;

        public HarryPotterController(ILogger<HarryPotterController> logger, IStocksProvider harryPotterService)
        {
            _logger = logger;
            this.harryPotterService = harryPotterService;
        }

        [HttpGet("characters")]
        public async Task<IActionResult> GetCharacters()
        {
            return GetUnWrappedResult(await harryPotterService.GetCharacters());
        }

        [HttpGet("spells")]
        public async Task<IActionResult> GetSpells()
        {
            return GetUnWrappedResult(await harryPotterService.GetSpells());
        }

        private IActionResult GetUnWrappedResult<T>(T result)
            where T : class
        {
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}
