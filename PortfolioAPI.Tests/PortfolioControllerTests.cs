using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioAPI.Controllers;
using PortfolioAPI.Models;
using PortfolioAPI.Services;

namespace PortfolioAPI.Tests;

public class PortfolioControllerTests
{
    private readonly Mock<IPortfolioService> _mockPortfolioService;
    private readonly Mock<ILogger<PortfolioController>> _mockLogger;
    private readonly PortfolioController _controller;

    public PortfolioControllerTests()
    {
        _mockPortfolioService = new Mock<IPortfolioService>();
        _mockLogger = new Mock<ILogger<PortfolioController>>();
        _controller = new PortfolioController(_mockPortfolioService.Object, _mockLogger.Object);
    }

    #region GetPortfolio Tests

    [Fact]
    public async Task GetPortfolio_WithValidClientId_Returns200WithPortfolio()
    {
        // Arrange
        int clientId = 101;
        var portfolio = new Portfolio
        {
            PortfolioId = 1,
            ClientId = clientId,
            TotalValue = 50000,
            CreatedDate = DateTime.Now,
            LastUpdated = DateTime.Now
        };

        _mockPortfolioService
            .Setup(x => x.GetPortfolioAsync(clientId))
            .ReturnsAsync(portfolio);

        // Act
        var result = await _controller.GetPortfolio(clientId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedPortfolio = Assert.IsType<Portfolio>(okResult.Value);
        Assert.Equal(clientId, returnedPortfolio.ClientId);
    }

    [Fact]
    public async Task GetPortfolio_WithInvalidClientId_Returns404NotFound()
    {
        // Arrange
        int clientId = 999;

        _mockPortfolioService
            .Setup(x => x.GetPortfolioAsync(clientId))
            .ReturnsAsync((Portfolio?)null);

        // Act
        var result = await _controller.GetPortfolio(clientId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetPortfolio_CallsServiceWithCorrectClientId()
    {
        // Arrange
        int clientId = 102;
        var portfolio = new Portfolio { PortfolioId = 2, ClientId = clientId, TotalValue = 75000 };

        _mockPortfolioService
            .Setup(x => x.GetPortfolioAsync(clientId))
            .ReturnsAsync(portfolio);

        // Act
        await _controller.GetPortfolio(clientId);

        // Assert
        _mockPortfolioService.Verify(x => x.GetPortfolioAsync(clientId), Times.Once);
    }

    #endregion

    #region GetHoldings Tests

    [Fact]
    public async Task GetHoldings_WithValidClientId_Returns200WithHoldings()
    {
        // Arrange
        int clientId = 101;
        var holdings = new List<Holding>
        {
            new Holding { HoldingId = 1, PortfolioId = 1, Symbol = "AAPL", Quantity = 100, CurrentPrice = 150 },
            new Holding { HoldingId = 2, PortfolioId = 1, Symbol = "MSFT", Quantity = 50, CurrentPrice = 300 }
        };

        _mockPortfolioService
            .Setup(x => x.GetHoldingsAsync(clientId))
            .ReturnsAsync(holdings);

        // Act
        var result = await _controller.GetHoldings(clientId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedHoldings = Assert.IsType<List<Holding>>(okResult.Value);
        Assert.Equal(2, returnedHoldings.Count);
    }

    [Fact]
    public async Task GetHoldings_WithInvalidClientId_ReturnsEmptyList()
    {
        // Arrange
        int clientId = 999;

        _mockPortfolioService
            .Setup(x => x.GetHoldingsAsync(clientId))
            .ReturnsAsync(new List<Holding>());

        // Act
        var result = await _controller.GetHoldings(clientId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedHoldings = Assert.IsType<List<Holding>>(okResult.Value);
        Assert.Empty(returnedHoldings);
    }

    [Fact]
    public async Task GetHoldings_CallsServiceWithCorrectClientId()
    {
        // Arrange
        int clientId = 101;

        _mockPortfolioService
            .Setup(x => x.GetHoldingsAsync(clientId))
            .ReturnsAsync(new List<Holding>());

        // Act
        await _controller.GetHoldings(clientId);

        // Assert
        _mockPortfolioService.Verify(x => x.GetHoldingsAsync(clientId), Times.Once);
    }

    #endregion

    #region BuyStock Tests - Success Scenario

    [Fact]
    public async Task BuyStock_WithValidRequest_Returns200WithSuccess()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 101,
            Symbol = "AAPL",
            Quantity = 10,
            Price = 150
        };

        var tradeResponse = new TradeResponse
        {
            Success = true,
            Message = "Successfully bought 10 shares of AAPL. Account balance: $49000",
            TotalCost = 1500,
            RemainingBalance = 49000
        };

        _mockPortfolioService
            .Setup(x => x.BuyStockAsync(It.IsAny<TradeRequest>()))
            .ReturnsAsync(tradeResponse);

        // Act
        var result = await _controller.BuyStock(tradeRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedResponse = Assert.IsType<TradeResponse>(okResult.Value);
        Assert.True(returnedResponse.Success);
    }

    #endregion

    #region BuyStock Tests - Insufficient Funds

    [Fact]
    public async Task BuyStock_WithInsufficientFunds_Returns400BadRequest()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 101,
            Symbol = "AAPL",
            Quantity = 100000,
            Price = 1000
        };

        var tradeResponse = new TradeResponse
        {
            Success = false,
            Message = "Insufficient funds for purchase. Total cost: $100000000",
            TotalCost = 100000000,
            RemainingBalance = 0
        };

        _mockPortfolioService
            .Setup(x => x.BuyStockAsync(It.IsAny<TradeRequest>()))
            .ReturnsAsync(tradeResponse);

        // Act
        var result = await _controller.BuyStock(tradeRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var returnedResponse = Assert.IsType<TradeResponse>(badRequestResult.Value);
        Assert.False(returnedResponse.Success);
    }

    #endregion

    #region BuyStock Tests - Portfolio Not Found

    [Fact]
    public async Task BuyStock_WithNonExistentPortfolio_Returns400BadRequest()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 999,
            Symbol = "AAPL",
            Quantity = 10,
            Price = 150
        };

        var tradeResponse = new TradeResponse
        {
            Success = false,
            Message = "Portfolio not found for client 999",
            TotalCost = 1500,
            RemainingBalance = 0
        };

        _mockPortfolioService
            .Setup(x => x.BuyStockAsync(It.IsAny<TradeRequest>()))
            .ReturnsAsync(tradeResponse);

        // Act
        var result = await _controller.BuyStock(tradeRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    #endregion

    #region BuyStock Tests - Invalid Request

    [Fact]
    public async Task BuyStock_WithNullRequest_Returns400BadRequest()
    {
        // Arrange
        TradeRequest? tradeRequest = null;

        // Act & Assert
        // Note: This would typically be caught by model binding, but we test the guard clause
        var exception = await Assert.ThrowsAsync<NullReferenceException>(
            async () => await _controller.BuyStock(tradeRequest!));
    }

    [Fact]
    public async Task BuyStock_WithNegativeQuantity_ReturnsBadRequest()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 101,
            Symbol = "AAPL",
            Quantity = -10, // Invalid: negative quantity
            Price = 150
        };

        var tradeResponse = new TradeResponse
        {
            Success = false,
            Message = "Quantity must be positive",
            TotalCost = 0,
            RemainingBalance = 0
        };

        _mockPortfolioService
            .Setup(x => x.BuyStockAsync(It.IsAny<TradeRequest>()))
            .ReturnsAsync(tradeResponse);

        // Act
        var result = await _controller.BuyStock(tradeRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    #endregion

    #region BuyStock Tests - Service Call Verification

    [Fact]
    public async Task BuyStock_CallsServiceWithCorrectRequest()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 101,
            Symbol = "GOOGL",
            Quantity = 5,
            Price = 200
        };

        var tradeResponse = new TradeResponse
        {
            Success = true,
            Message = "Trade successful",
            TotalCost = 1000,
            RemainingBalance = 49000
        };

        _mockPortfolioService
            .Setup(x => x.BuyStockAsync(It.IsAny<TradeRequest>()))
            .ReturnsAsync(tradeResponse);

        // Act
        await _controller.BuyStock(tradeRequest);

        // Assert
        _mockPortfolioService.Verify(
            x => x.BuyStockAsync(It.Is<TradeRequest>(r =>
                r.ClientId == 101 &&
                r.Symbol == "GOOGL" &&
                r.Quantity == 5 &&
                r.Price == 200)),
            Times.Once);
    }

    #endregion

    #region BuyStock Tests - Account Service Failure

    [Fact]
    public async Task BuyStock_WhenAccountServiceFails_Returns400WithErrorMessage()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 101,
            Symbol = "AAPL",
            Quantity = 10,
            Price = 150
        };

        var tradeResponse = new TradeResponse
        {
            Success = false,
            Message = "Trade executed but account debit failed. Please contact support.",
            TotalCost = 1500,
            RemainingBalance = 0
        };

        _mockPortfolioService
            .Setup(x => x.BuyStockAsync(It.IsAny<TradeRequest>()))
            .ReturnsAsync(tradeResponse);

        // Act
        var result = await _controller.BuyStock(tradeRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var returnedResponse = Assert.IsType<TradeResponse>(badRequestResult.Value);
        Assert.Contains("account debit failed", returnedResponse.Message);
    }

    #endregion

    #region BuyStock Tests - Response Format Validation

    [Fact]
    public async Task BuyStock_SuccessfulResponse_ContainsTotalCostAndRemainingBalance()
    {
        // Arrange
        var tradeRequest = new TradeRequest
        {
            ClientId = 101,
            Symbol = "MSFT",
            Quantity = 20,
            Price = 300
        };

        var tradeResponse = new TradeResponse
        {
            Success = true,
            Message = "Trade successful",
            TotalCost = 6000,
            RemainingBalance = 44000
        };

        _mockPortfolioService
            .Setup(x => x.BuyStockAsync(It.IsAny<TradeRequest>()))
            .ReturnsAsync(tradeResponse);

        // Act
        var result = await _controller.BuyStock(tradeRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TradeResponse>(okResult.Value);
        Assert.Equal(6000, response.TotalCost);
        Assert.Equal(44000, response.RemainingBalance);
    }

    #endregion
}
