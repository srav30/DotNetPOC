using Microsoft.Extensions.Logging;
using Moq;
using AccountAPI.Models;
using AccountAPI.Repositories;
using AccountAPI.Services;

namespace AccountAPI.Tests;

public class AccountFileServiceTests
{
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<IAccountFileRepository> _mockAccountFileRepository;
    private readonly Mock<ILogger<AccountFileService>> _mockLogger;
    private readonly AccountFileService _service;

    public AccountFileServiceTests()
    {
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockAccountFileRepository = new Mock<IAccountFileRepository>();
        _mockLogger = new Mock<ILogger<AccountFileService>>();
        _service = new AccountFileService(_mockAccountRepository.Object, _mockAccountFileRepository.Object, _mockLogger.Object);
    }

    #region GenerateAndSaveAccountStatementAsync Tests

    [Fact]
    public async Task GenerateAndSaveAccountStatementAsync_WithValidAccount_SavesAndReturnsCsvFile()
    {
        // Arrange
        var account = new Account
        {
            AccountId = 1,
            ClientId = 101,
            AccountType = "Investment",
            Balance = 50000m,
            IsActive = true,
            CreatedDate = new DateTime(2024, 1, 15)
        };

        var savedFile = new AccountFile
        {
            FileId = 1,
            AccountId = 1,
            FileName = $"Account_Statement_{account.ClientId}_20260202_120000.csv",
            FileType = "csv",
            FileContent = new byte[] { 65, 99, 99 }, // dummy bytes
            CreatedDate = DateTime.UtcNow
        };

        _mockAccountRepository
            .Setup(r => r.GetAccountByClientIdAsync(account.ClientId))
            .ReturnsAsync(account);

        _mockAccountFileRepository
            .Setup(r => r.SaveAccountFileAsync(It.IsAny<int>(), It.IsAny<string>(), "csv", It.IsAny<byte[]>()))
            .ReturnsAsync(savedFile);

        // Act
        var result = await _service.GenerateAndSaveAccountStatementAsync(account.ClientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("csv", result.FileType);
        Assert.NotEmpty(result.FileContent);
        _mockAccountFileRepository.Verify(
            r => r.SaveAccountFileAsync(It.IsAny<int>(), It.IsAny<string>(), "csv", It.IsAny<byte[]>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateAndSaveAccountStatementAsync_WithInvalidAccountId_ReturnsNull()
    {
        // Arrange
        int invalidClientId = 999;
        _mockAccountRepository
            .Setup(r => r.GetAccountByClientIdAsync(invalidClientId))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.GenerateAndSaveAccountStatementAsync(invalidClientId);

        // Assert
        Assert.Null(result);
        _mockAccountFileRepository.Verify(
            r => r.SaveAccountFileAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()),
            Times.Never);
    }

    [Fact]
    public async Task GenerateAndSaveAccountStatementAsync_CsvContainsAccountData()
    {
        // Arrange
        var account = new Account
        {
            AccountId = 1,
            ClientId = 101,
            AccountType = "Investment",
            Balance = 50000m,
            IsActive = true,
            CreatedDate = new DateTime(2024, 1, 15)
        };

        _mockAccountRepository
            .Setup(r => r.GetAccountByClientIdAsync(account.ClientId))
            .ReturnsAsync(account);

        byte[]? capturedFileContent = null;

        _mockAccountFileRepository
            .Setup(r => r.SaveAccountFileAsync(It.IsAny<int>(), It.IsAny<string>(), "csv", It.IsAny<byte[]>()))
            .Callback<int, string, string, byte[]>((accountId, fileName, fileType, fileContent) =>
            {
                capturedFileContent = fileContent;
            })
            .ReturnsAsync(new AccountFile { FileId = 1, AccountId = 1, FileContent = new byte[] { } });

        // Act
        await _service.GenerateAndSaveAccountStatementAsync(account.ClientId);

        // Assert
        Assert.NotNull(capturedFileContent);
        var csvContent = System.Text.Encoding.UTF8.GetString(capturedFileContent);
        Assert.Contains("AccountId,ClientId,AccountType,Balance,IsActive,CreatedDate", csvContent);
        Assert.Contains("101", csvContent); // ClientId should be in CSV
        Assert.Contains("50000.00", csvContent); // Balance should be in CSV
        Assert.Contains("Investment", csvContent); // AccountType should be in CSV
    }

    [Fact]
    public async Task GenerateAndSaveAccountStatementAsync_FileNameIncludesClientIdAndTimestamp()
    {
        // Arrange
        var account = new Account
        {
            AccountId = 1,
            ClientId = 101,
            AccountType = "Investment",
            Balance = 50000m,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        string? capturedFileName = null;

        _mockAccountRepository
            .Setup(r => r.GetAccountByClientIdAsync(account.ClientId))
            .ReturnsAsync(account);

        _mockAccountFileRepository
            .Setup(r => r.SaveAccountFileAsync(It.IsAny<int>(), It.IsAny<string>(), "csv", It.IsAny<byte[]>()))
            .Callback<int, string, string, byte[]>((accountId, fileName, fileType, fileContent) =>
            {
                capturedFileName = fileName;
            })
            .ReturnsAsync(new AccountFile { FileId = 1, AccountId = 1, FileContent = new byte[] { } });

        // Act
        await _service.GenerateAndSaveAccountStatementAsync(account.ClientId);

        // Assert
        Assert.NotNull(capturedFileName);
        Assert.StartsWith("Account_Statement_101", capturedFileName);
        Assert.EndsWith(".csv", capturedFileName);
    }

    #endregion
}
