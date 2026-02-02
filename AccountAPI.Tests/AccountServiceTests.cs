using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using AccountAPI.Models;
using AccountAPI.Repositories;
using AccountAPI.Services;

namespace AccountAPI.Tests;

public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _mockRepository;
    private readonly Mock<ILogger<AccountService>> _mockLogger;
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _mockRepository = new Mock<IAccountRepository>();
        _mockLogger = new Mock<ILogger<AccountService>>();
        _service = new AccountService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAccountAsync_WithValidClientId_ReturnsAccount()
    {
        // Arrange
        var clientId = 101;
        var expectedAccount = new Account
        {
            AccountId = 1,
            ClientId = clientId,
            Balance = 50000,
            AccountType = "Investment",
            IsActive = true,
            CreatedDate = DateTime.Now
        };
        _mockRepository.Setup(r => r.GetAccountByClientIdAsync(clientId))
            .ReturnsAsync(expectedAccount);

        // Act
        var result = await _service.GetAccountAsync(clientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(clientId, result.ClientId);
        Assert.Equal(50000, result.Balance);
        _mockRepository.Verify(r => r.GetAccountByClientIdAsync(clientId), Times.Once);
    }

    [Fact]
    public async Task GetAccountAsync_WithInvalidClientId_ReturnsNull()
    {
        // Arrange
        var clientId = 999;
        _mockRepository.Setup(r => r.GetAccountByClientIdAsync(clientId))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.GetAccountAsync(clientId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WithdrawAsync_WithValidWithdrawal_ReturnsUpdatedAccount()
    {
        // Arrange
        var clientId = 101;
        var amount = 5000m;
        var withdrawnAccount = new Account
        {
            AccountId = 1,
            ClientId = clientId,
            Balance = 45000,
            AccountType = "Investment",
            IsActive = true,
            CreatedDate = DateTime.Now
        };
        _mockRepository.Setup(r => r.WithdrawAsync(clientId, amount))
            .ReturnsAsync(withdrawnAccount);

        // Act
        var result = await _service.WithdrawAsync(clientId, amount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(45000, result.Balance);
        _mockRepository.Verify(r => r.WithdrawAsync(clientId, amount), Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_WithInsufficientFunds_ReturnsNull()
    {
        // Arrange
        var clientId = 101;
        var amount = 100000m;
        _mockRepository.Setup(r => r.WithdrawAsync(clientId, amount))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.WithdrawAsync(clientId, amount);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DepositAsync_WithValidDeposit_ReturnsUpdatedAccount()
    {
        // Arrange
        var clientId = 101;
        var amount = 10000m;
        var depositedAccount = new Account
        {
            AccountId = 1,
            ClientId = clientId,
            Balance = 60000,
            AccountType = "Investment",
            IsActive = true,
            CreatedDate = DateTime.Now
        };
        _mockRepository.Setup(r => r.DepositAsync(clientId, amount))
            .ReturnsAsync(depositedAccount);

        // Act
        var result = await _service.DepositAsync(clientId, amount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(60000, result.Balance);
        _mockRepository.Verify(r => r.DepositAsync(clientId, amount), Times.Once);
    }

    [Fact]
    public async Task DepositAsync_WithInvalidClientId_ReturnsNull()
    {
        // Arrange
        var clientId = 999;
        var amount = 10000m;
        _mockRepository.Setup(r => r.DepositAsync(clientId, amount))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.DepositAsync(clientId, amount);

        // Assert
        Assert.Null(result);
    }
}
