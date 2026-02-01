using Microsoft.AspNetCore.Mvc;
using PortfolioAPI.Models;

namespace PortfolioAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PortfolioController> _logger;

    // Mock database
    private static readonly Dictionary<int, Portfolio> _portfolios = new()
    {
        { 1, new Portfolio { PortfolioId = 1, ClientId = 101, TotalValue = 50000, CreatedDate = DateTime.Now, LastUpdated = DateTime.Now } },
        { 2, new Portfolio { PortfolioId = 2, ClientId = 102, TotalValue = 75000, CreatedDate = DateTime.Now, LastUpdated = DateTime.Now } }
    };

    private static readonly Dictionary<int, Holding> _holdings = new()
    {
        { 1, new Holding { HoldingId = 1, PortfolioId = 1, Symbol = "AAPL", Quantity = 100, CurrentPrice = 150 } },
        { 2, new Holding { HoldingId = 2, PortfolioId = 1, Symbol = "MSFT", Quantity = 50, CurrentPrice = 300 } }
    };

    public PortfolioController(HttpClient httpClient, ILogger<PortfolioController> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [HttpGet("{clientId}")]
    public ActionResult<Portfolio> GetPortfolio(int clientId)
    {
        var portfolio = _portfolios.Values.FirstOrDefault(p => p.ClientId == clientId);
        if (portfolio == null)
            return NotFound($"Portfolio not found for client {clientId}");
        return Ok(portfolio);
    }

    [HttpGet("{clientId}/holdings")]
    public ActionResult<IEnumerable<Holding>> GetHoldings(int clientId)
    {
        var portfolio = _portfolios.Values.FirstOrDefault(p => p.ClientId == clientId);
        if (portfolio == null)
            return NotFound($"Portfolio not found for client {clientId}");

        var holdings = _holdings.Values.Where(h => h.PortfolioId == portfolio.PortfolioId);
        return Ok(holdings);
    }

    [HttpPost("buy")]
    public async Task<ActionResult<TradeResponse>> BuyStock([FromBody] TradeRequest request)
    {
        try
        {
            // Step 1: Call Account Service to verify funds
            var accountServiceUrl = "http://localhost:5132/api/accounts/verify-funds";
            var verificationRequest = new { request.ClientId, RequiredAmount = request.TotalCost };
            
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(verificationRequest);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(accountServiceUrl, content);

            if (!response.IsSuccessStatusCode)
                return BadRequest("Account Service verification failed");

            var jsonString = await response.Content.ReadAsStringAsync();
            var hasFunds = System.Text.Json.JsonSerializer.Deserialize<bool>(jsonString);
            if (!hasFunds)
                return BadRequest($"Insufficient funds for purchase. Total cost: ${request.TotalCost}");

            // Step 2: Add holding to portfolio
            var portfolio = _portfolios.Values.FirstOrDefault(p => p.ClientId == request.ClientId);
            if (portfolio == null)
                return NotFound($"Portfolio not found for client {request.ClientId}");

            var newHolding = new Holding
            {
                HoldingId = _holdings.Count + 1,
                PortfolioId = portfolio.PortfolioId,
                Symbol = request.Symbol,
                Quantity = request.Quantity,
                CurrentPrice = request.Price
            };

            _holdings.Add(newHolding.HoldingId, newHolding);
            portfolio.TotalValue += newHolding.TotalValue;
            portfolio.LastUpdated = DateTime.Now;

            // Step 3: Debit the account with the trade amount
            var withdrawUrl = "http://localhost:5132/api/accounts/withdraw";
            var withdrawRequest = new { request.ClientId, Amount = request.TotalCost };
            
            var withdrawJsonContent = System.Text.Json.JsonSerializer.Serialize(withdrawRequest);
            var withdrawContent = new StringContent(withdrawJsonContent, System.Text.Encoding.UTF8, "application/json");
            
            var withdrawResponse = await _httpClient.PostAsync(withdrawUrl, withdrawContent);
            
            if (!withdrawResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to withdraw funds from account after trade");
                return BadRequest("Trade executed but account debit failed. Please contact support.");
            }

            var withdrawJsonString = await withdrawResponse.Content.ReadAsStringAsync();
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var updatedAccount = System.Text.Json.JsonSerializer.Deserialize<AccountInfo>(withdrawJsonString, options);

            return Ok(new TradeResponse 
            { 
                Success = true, 
                Message = $"Successfully bought {request.Quantity} shares of {request.Symbol}. Account balance: ${updatedAccount?.Balance}",
                TotalCost = request.TotalCost,
                RemainingBalance = updatedAccount?.Balance ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing trade");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class AccountInfo
{
    public int AccountId { get; set; }
    public int ClientId { get; set; }
    public string? AccountType { get; set; }
    public decimal Balance { get; set; }
}

public class TradeResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public decimal TotalCost { get; set; }
    public decimal RemainingBalance { get; set; }
}
