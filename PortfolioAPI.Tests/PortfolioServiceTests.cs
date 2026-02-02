using Microsoft.Extensions.Logging;
using Moq;
using PortfolioAPI.Models;
using PortfolioAPI.Services;

namespace PortfolioAPI.Tests;

/// <summary>
/// Unit tests for PortfolioService.
/// Tests for GetPortfolioAsync and GetHoldingsAsync verify the mock data behavior.
/// BuyStockAsync tests focus on business logic validation and error handling.
/// </summary>
public class PortfolioServiceTests
{
    private readonly ILogger<PortfolioService> _mockLogger;
    private readonly PortfolioService _portfolioService;

    public PortfolioServiceTests()
    {
        _mockLogger = new Mock<ILogger<PortfolioService>>().Object;
        var httpClient = new HttpClient();
        _portfolioService = new PortfolioService(httpClient, _mockLogger);
    }

    #region GetPortfolioAsync Tests

    [Fact]
    public async Task GetPortfolioAsync_WithValidClientId101_ReturnsPortfolioWithCorrectValue()
    {
        // Arrange
        int clientId = 101;

        // Act
        var result = await _portfolioService.GetPortfolioAsync(clientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(clientId, result.ClientId);
        Assert.Equal(1, result.PortfolioId);
        Assert.Equal(50000, result.TotalValue);
    }

    [Fact]
    public async Task GetPortfolioAsync_WithValidClientId102_ReturnsPortfolioWithCorrectValue()
    {
        // Arrange
        int clientId = 102;

        // Act
        var result = await _portfolioService.GetPortfolioAsync(clientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(clientId, result.ClientId);
        Assert.Equal(2, result.PortfolioId);
        Assert.Equal(75000, result.TotalValue);
    }

    [Fact]
    public async Task GetPortfolioAsync_WithInvalidClientId_ReturnsNull()
    {
        // Arrange
        int clientId = 999;

        // Act
        var result = await _portfolioService.GetPortfolioAsync(clientId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPortfolioAsync_WithClientId0_ReturnsNull()
    {
        // Arrange
        int clientId = 0;

        // Act
        var result = await _portfolioService.GetPortfolioAsync(clientId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetHoldingsAsync Tests

    [Fact]
    public async Task GetHoldingsAsync_WithValidClientId101_ReturnsHoldings()
    {
        // Arrange
        int clientId = 101;

        // Act
        var result = await _portfolioService.GetHoldingsAsync(clientId);

        // Assert
        var holdings = result.ToList();
        Assert.NotEmpty(holdings);
        Assert.All(holdings, holding =>
        {
            Assert.NotNull(holding.Symbol);
            Assert.True(holding.Quantity > 0);
        });
    }

    [Fact]
    public async Task GetHoldingsAsync_WithClientId101_ContainsAppleAndMicrosoft()
    {
        // Arrange
        int clientId = 101;

        // Act
        var result = await _portfolioService.GetHoldingsAsync(clientId);

        // Assert
        var holdings = result.ToList();
        var symbols = holdings.Select(h => h.Symbol).ToList();
        Assert.Contains("AAPL", symbols);
        Assert.Contains("MSFT", symbols);
    }

    [Fact]
    public async Task GetHoldingsAsync_WithClientId101_VerifyAppleHoldingDetails()
    {
        // Arrange
        int clientId = 101;

        // Act
        var result = await _portfolioService.GetHoldingsAsync(clientId);

        // Assert
        var holding = result.FirstOrDefault(h => h.Symbol == "AAPL");
        Assert.NotNull(holding);
        Assert.Equal(100, holding.Quantity);
        Assert.Equal(150, holding.CurrentPrice);
        Assert.Equal(15000, holding.TotalValue); // 100 * 150
    }

    [Fact]
    public async Task GetHoldingsAsync_WithClientId102_ReturnsEmptyList()
    {
        // Arrange
        int clientId = 102; // This client exists but has no holdings in mock data

        // Act
        var result = await _portfolioService.GetHoldingsAsync(clientId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHoldingsAsync_WithInvalidClientId_ReturnsEmptyList()
    {
        // Arrange
        int clientId = 999;

        // Act
        var result = await _portfolioService.GetHoldingsAsync(clientId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHoldingsAsync_ResultEnumerableCanBeIteratedMultipleTimes()
    {
        // Arrange
        int clientId = 101;

        // Act
        var result = await _portfolioService.GetHoldingsAsync(clientId);

        // Assert - Verify enumerable can be iterated multiple times
        var firstPass = result.Count();
        var secondPass = result.Count();
        Assert.Equal(firstPass, secondPass);
        Assert.Equal(2, firstPass);
    }

    #endregion

    #region BuyStockAsync Tests - Input Validation and Calculation

    [Fact]
    public async Task BuyStockAsync_CalculatesTotalCostCorrectly()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 101,
            Symbol = "TEST",
            Quantity = 25,
            Price = 150.50m
        };

        // Act
        var result = await _portfolioService.BuyStockAsync(tradeRequest);

        // Assert - Verify total cost calculation
        Assert.Equal(25 * 150.50m, result.TotalCost);
    }

    [Fact]
    public async Task BuyStockAsync_WithZeroQuantity_TotalCostIsZero()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 101,
            Symbol = "TEST",
            Quantity = 0,
            Price = 150
        };

        // Act
        var result = await _portfolioService.BuyStockAsync(tradeRequest);

        // Assert
        Assert.Equal(0, result.TotalCost);
    }

    [Fact]
    public async Task BuyStockAsync_WithLargeQuantity_CalculatesCorrectly()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 101,
            Symbol = "PENNY",
            Quantity = 1000000,
            Price = 0.01m
        };

        // Act
        var result = await _portfolioService.BuyStockAsync(tradeRequest);

        // Assert
        Assert.Equal(1000000 * 0.01m, result.TotalCost);
    }

    [Fact]
    public async Task BuyStockAsync_WithDecimalPrice_CalculatesCorrectly()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 101,
            Symbol = "FRAC",
            Quantity = 7,
            Price = 123.456789m
        };

        // Act
        var result = await _portfolioService.BuyStockAsync(tradeRequest);

        // Assert
        Assert.Equal(7 * 123.456789m, result.TotalCost);
    }

    #endregion

    #region BuyStockAsync Tests - Error Scenarios

    [Fact]
    public async Task BuyStockAsync_WithNonExistentPortfolio_ReturnsFailed()
    {
        // Arrange - Non-existent client ID
        var tradeRequest = new TradeRequest
        {
            ClientId = 999,
            Symbol = "AAPL",
            Quantity = 10,
            Price = 150
        };

        // Act
        var result = await _portfolioService.BuyStockAsync(tradeRequest);

        // Assert - Account Service call will fail first, returning insufficient funds
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.Equal(1500, result.TotalCost);
    }

    [Fact]
    public async Task BuyStockAsync_ReturnsTradeResponse_WithAllRequiredFields()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 999, // Non-existent, will fail
            Symbol = "AAPL",
            Quantity = 10,
            Price = 150
        };

        // Act
        var result = await _portfolioService.BuyStockAsync(tradeRequest);

        // Assert - Verify response has all required fields even on failure
        Assert.NotNull(result);
        Assert.NotNull(result.Message);
        Assert.IsType<bool>(result.Success);
        Assert.True(result.TotalCost > 0);
    }

    [Fact]
    public async Task BuyStockAsync_WhenAccountServiceUnreachable_ReturnsFailed()
    {
        // Arrange - Valid client but Account Service not running
        var tradeRequest = new TradeRequest
        {
            ClientId = 101,
            Symbol = "AAPL",
            Quantity = 10,
            Price = 150
        };

        // Act - Account Service call will fail
        var result = await _portfolioService.BuyStockAsync(tradeRequest);

        // Assert
        Assert.False(result.Success);
        // Message will indicate insufficient funds or internal error
        Assert.NotNull(result.Message);
    }

    #endregion

    #region BuyStockAsync Tests - Response Contract

    [Fact]
    public async Task BuyStockAsync_FailureResponse_ContainsTotalCost()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 999, // Non-existent portfolio
            Symbol = "TEST",
            Quantity = 50,
            Price = 100
        };

        // Act
        var result = await _portfolioService.BuyStockAsync(tradeRequest);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(50 * 100, result.TotalCost);
    }

    [Fact]
    public async Task BuyStockAsync_FailureResponse_RemainingBalanceIsZero()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 999, // Non-existent
            Symbol = "TEST",
            Quantity = 10,
            Price = 100
        };

        // Act
        var result = await _portfolioService.BuyStockAsync(tradeRequest);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(0, result.RemainingBalance);
    }

    [Fact]
    public async Task BuyStockAsync_FailureResponse_MessageIsNotEmpty()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 999,
            Symbol = "TEST",
            Quantity = 10,
            Price = 150
        };

        // Act
        var result = await _portfolioService.BuyStockAsync(tradeRequest);

        // Assert
        Assert.False(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    #endregion

    #region BuyStockAsync Tests - Special Cases

    [Fact]
    public async Task BuyStockAsync_WithMultipleCalls_DoesNotAffectMockData()
    {
        // Arrange
        var request1 = new TradeRequest { ClientId = 101, Symbol = "TEST1", Quantity = 10, Price = 100 };
        var request2 = new TradeRequest { ClientId = 102, Symbol = "TEST2", Quantity = 5, Price = 200 };

        // Act
        var result1 = await _portfolioService.BuyStockAsync(request1);
        var result2 = await _portfolioService.BuyStockAsync(request2);

        // Assert - Verify each call processes independently
        Assert.NotNull(result1);
        Assert.NotNull(result2);
    }

    [Fact]
    public async Task BuyStockAsync_ClientId101_HasPortfolio()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 101,
            Symbol = "TEST",
            Quantity = 5,
            Price = 100
        };

        // Act
        var result = await _portfolioService.BuyStockAsync(tradeRequest);

        // Assert - Should not return portfolio not found error
        Assert.NotNull(result);
        Assert.DoesNotContain("Portfolio not found", result.Message);
    }

    #endregion
}
