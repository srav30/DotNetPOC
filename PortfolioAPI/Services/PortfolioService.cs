using PortfolioAPI.Models;
using System.Text.Json;

namespace PortfolioAPI.Services;

public class PortfolioService : IPortfolioService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PortfolioService> _logger;

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

    public PortfolioService(HttpClient httpClient, ILogger<PortfolioService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<Portfolio?> GetPortfolioAsync(int clientId)
    {
        var portfolio = _portfolios.Values.FirstOrDefault(p => p.ClientId == clientId);
        return Task.FromResult(portfolio);
    }

    public Task<IEnumerable<Holding>> GetHoldingsAsync(int clientId)
    {
        var portfolio = _portfolios.Values.FirstOrDefault(p => p.ClientId == clientId);
        if (portfolio == null)
            return Task.FromResult(Enumerable.Empty<Holding>());

        var holdings = _holdings.Values.Where(h => h.PortfolioId == portfolio.PortfolioId);
        return Task.FromResult(holdings);
    }

    public async Task<TradeResponse> BuyStockAsync(TradeRequest request)
    {
        try
        {
            // Step 1: Verify funds with Account Service
            var hasFunds = await VerifyFundsWithAccountServiceAsync(request.ClientId, request.TotalCost);
            if (!hasFunds)
            {
                return new TradeResponse
                {
                    Success = false,
                    Message = $"Insufficient funds for purchase. Total cost: ${request.TotalCost}",
                    TotalCost = request.TotalCost,
                    RemainingBalance = 0
                };
            }

            // Step 2: Find portfolio
            var portfolio = _portfolios.Values.FirstOrDefault(p => p.ClientId == request.ClientId);
            if (portfolio == null)
            {
                return new TradeResponse
                {
                    Success = false,
                    Message = $"Portfolio not found for client {request.ClientId}",
                    TotalCost = request.TotalCost,
                    RemainingBalance = 0
                };
            }

            // Step 3: Add holding to portfolio
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

            // Step 4: Debit the account with the trade amount
            var accountInfo = await WithdrawFromAccountAsync(request.ClientId, request.TotalCost);
            if (accountInfo == null)
            {
                _logger.LogError("Failed to withdraw funds from account after trade");
                return new TradeResponse
                {
                    Success = false,
                    Message = "Trade executed but account debit failed. Please contact support.",
                    TotalCost = request.TotalCost,
                    RemainingBalance = 0
                };
            }

            return new TradeResponse
            {
                Success = true,
                Message = $"Successfully bought {request.Quantity} shares of {request.Symbol}. Account balance: ${accountInfo.Balance}",
                TotalCost = request.TotalCost,
                RemainingBalance = accountInfo.Balance
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing trade");
            return new TradeResponse
            {
                Success = false,
                Message = "Internal server error",
                TotalCost = request.TotalCost,
                RemainingBalance = 0
            };
        }
    }

    /// <summary>
    /// Calls Account Service to verify if client has sufficient funds
    /// </summary>
    private async Task<bool> VerifyFundsWithAccountServiceAsync(int clientId, decimal requiredAmount)
    {
        try
        {
            var accountServiceUrl = "http://localhost:5132/api/accounts/verify-funds";
            var verificationRequest = new { ClientId = clientId, RequiredAmount = requiredAmount };

            var jsonContent = JsonSerializer.Serialize(verificationRequest);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(accountServiceUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Account Service verification failed with status code {StatusCode}", response.StatusCode);
                return false;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var hasFunds = JsonSerializer.Deserialize<bool>(jsonString);
            return hasFunds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Account Service for fund verification");
            return false;
        }
    }

    /// <summary>
    /// Calls Account Service to withdraw funds from client account
    /// </summary>
    private async Task<AccountInfo?> WithdrawFromAccountAsync(int clientId, decimal amount)
    {
        try
        {
            var withdrawUrl = "http://localhost:5132/api/accounts/withdraw";
            var withdrawRequest = new { ClientId = clientId, Amount = amount };

            var withdrawJsonContent = JsonSerializer.Serialize(withdrawRequest);
            var withdrawContent = new StringContent(withdrawJsonContent, System.Text.Encoding.UTF8, "application/json");

            var withdrawResponse = await _httpClient.PostAsync(withdrawUrl, withdrawContent);
            if (!withdrawResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Account Service withdrawal failed with status code {StatusCode}", withdrawResponse.StatusCode);
                return null;
            }

            var withdrawJsonString = await withdrawResponse.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var updatedAccount = JsonSerializer.Deserialize<AccountInfo>(withdrawJsonString, options);
            return updatedAccount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Account Service for withdrawal");
            return null;
        }
    }
}
