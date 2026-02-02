using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AccountAPI.Models;
using AccountAPI.Services;
using AccountAPI.Controllers;

namespace AccountAPI.Tests;

public class AccountsControllerTests
{
    private readonly Mock<IAccountService> _mockService;
    private readonly Mock<ILogger<AccountsController>> _mockLogger;
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _mockService = new Mock<IAccountService>();
        _mockLogger = new Mock<ILogger<AccountsController>>();
        _controller = new AccountsController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAccount_WithValidClientId_ReturnsOkWithAccount()
    {
        // Arrange
        var clientId = 101;
        var account = new Account
        {
            AccountId = 1,
            ClientId = clientId,
            Balance = 50000,
            AccountType = "Investment",
            IsActive = true,
            CreatedDate = DateTime.Now
        };
        _mockService.Setup(s => s.GetAccountAsync(clientId))
            .ReturnsAsync(account);

        // Act
        var result = await _controller.GetAccount(clientId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedAccount = Assert.IsType<Account>(okResult.Value);
        Assert.Equal(clientId, returnedAccount.ClientId);
    }

    [Fact]
    public async Task GetAccount_WithInvalidClientId_ReturnsNotFound()
    {
        // Arrange
        var clientId = 999;
        _mockService.Setup(s => s.GetAccountAsync(clientId))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _controller.GetAccount(clientId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Withdraw_WithValidAmount_ReturnsOkWithUpdatedAccount()
    {
        // Arrange
        var request = new WithdrawalRequest { ClientId = 101, Amount = 5000 };
        var withdrawnAccount = new Account
        {
            AccountId = 1,
            ClientId = 101,
            Balance = 45000,
            AccountType = "Investment",
            IsActive = true,
            CreatedDate = DateTime.Now
        };
        _mockService.Setup(s => s.WithdrawAsync(request.ClientId, request.Amount))
            .ReturnsAsync(withdrawnAccount);

        // Act
        var result = await _controller.Withdraw(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAccount = Assert.IsType<Account>(okResult.Value);
        Assert.Equal(45000, returnedAccount.Balance);
    }

    [Fact]
    public async Task Withdraw_WithInsufficientFunds_ReturnsBadRequest()
    {
        // Arrange
        var request = new WithdrawalRequest { ClientId = 101, Amount = 100000 };
        var existingAccount = new Account
        {
            AccountId = 1,
            ClientId = 101,
            Balance = 50000,
            AccountType = "Investment",
            IsActive = true,
            CreatedDate = DateTime.Now
        };
        _mockService.Setup(s => s.WithdrawAsync(request.ClientId, request.Amount))
            .ReturnsAsync((Account?)null);
        _mockService.Setup(s => s.GetAccountAsync(request.ClientId))
            .ReturnsAsync(existingAccount);

        // Act
        var result = await _controller.Withdraw(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Withdraw_WithInvalidClientId_ReturnsNotFound()
    {
        // Arrange
        var request = new WithdrawalRequest { ClientId = 999, Amount = 1000 };
        _mockService.Setup(s => s.WithdrawAsync(request.ClientId, request.Amount))
            .ReturnsAsync((Account?)null);
        _mockService.Setup(s => s.GetAccountAsync(request.ClientId))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _controller.Withdraw(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Deposit_WithValidAmount_ReturnsOkWithUpdatedAccount()
    {
        // Arrange
        var request = new DepositRequest { ClientId = 101, Amount = 10000 };
        var depositedAccount = new Account
        {
            AccountId = 1,
            ClientId = 101,
            Balance = 60000,
            AccountType = "Investment",
            IsActive = true,
            CreatedDate = DateTime.Now
        };
        _mockService.Setup(s => s.DepositAsync(request.ClientId, request.Amount))
            .ReturnsAsync(depositedAccount);

        // Act
        var result = await _controller.Deposit(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAccount = Assert.IsType<Account>(okResult.Value);
        Assert.Equal(60000, returnedAccount.Balance);
    }

    [Fact]
    public async Task Deposit_WithInvalidClientId_ReturnsNotFound()
    {
        // Arrange
        var request = new DepositRequest { ClientId = 999, Amount = 10000 };
        _mockService.Setup(s => s.DepositAsync(request.ClientId, request.Amount))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _controller.Deposit(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}
