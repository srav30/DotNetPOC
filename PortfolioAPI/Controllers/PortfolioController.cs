using Microsoft.AspNetCore.Mvc;
using PortfolioAPI.Models;
using PortfolioAPI.Services;

namespace PortfolioAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;
    private readonly ILogger<PortfolioController> _logger;

    public PortfolioController(IPortfolioService portfolioService, ILogger<PortfolioController> logger)
    {
        _portfolioService = portfolioService;
        _logger = logger;
    }

    [HttpGet("{clientId}")]
    public async Task<ActionResult<Portfolio>> GetPortfolio(int clientId)
    {
        var portfolio = await _portfolioService.GetPortfolioAsync(clientId);
        if (portfolio == null)
            return NotFound(new { message = $"Portfolio not found for client {clientId}" });

        return Ok(portfolio);
    }

    [HttpGet("{clientId}/holdings")]
    public async Task<ActionResult<IEnumerable<Holding>>> GetHoldings(int clientId)
    {
        var holdings = await _portfolioService.GetHoldingsAsync(clientId);
        return Ok(holdings);
    }

    [HttpPost("buy")]
    public async Task<ActionResult<TradeResponse>> BuyStock([FromBody] TradeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _portfolioService.BuyStockAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
