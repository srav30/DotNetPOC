using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using AccountAPI.Controllers;
using AccountAPI.Models;
using AccountAPI.Services;

namespace AccountAPI.Tests;

public class AccountFileControllerTests
{
    private readonly Mock<IAccountFileService> _mockAccountFileService;
    private readonly Mock<ILogger<AccountFileController>> _mockLogger;
    private readonly AccountFileController _controller;

    public AccountFileControllerTests()
    {
        _mockAccountFileService = new Mock<IAccountFileService>();
        _mockLogger = new Mock<ILogger<AccountFileController>>();
        _controller = new AccountFileController(_mockAccountFileService.Object, _mockLogger.Object);
    }

    #region GenerateAccountStatement Tests

    [Fact]
    public async Task GenerateAccountStatement_WithValidAccountId_ReturnsFileResult()
    {
        // Arrange
        int clientId = 101;
        var fileContent = new byte[] { 65, 66, 67 }; // ABC
        var accountFile = new AccountFile
        {
            FileId = 1,
            AccountId = 1,
            FileName = "Account_Statement_101_20260202.csv",
            FileType = "csv",
            FileContent = fileContent,
            CreatedDate = DateTime.UtcNow
        };

        _mockAccountFileService
            .Setup(s => s.GenerateAndSaveAccountStatementAsync(clientId))
            .ReturnsAsync(accountFile);

        // Act
        var result = await _controller.GenerateAccountStatement(clientId);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", fileResult.ContentType);
        Assert.Equal(accountFile.FileName, fileResult.FileDownloadName);
        Assert.Equal(fileContent, fileResult.FileContents);
    }

    [Fact]
    public async Task GenerateAccountStatement_WithInvalidAccountId_Returns404NotFound()
    {
        // Arrange
        int invalidClientId = 999;
        _mockAccountFileService
            .Setup(s => s.GenerateAndSaveAccountStatementAsync(invalidClientId))
            .ReturnsAsync((AccountFile?)null);

        // Act
        var result = await _controller.GenerateAccountStatement(invalidClientId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GenerateAccountStatement_CallsServiceWithCorrectAccountId()
    {
        // Arrange
        int clientId = 101;
        _mockAccountFileService
            .Setup(s => s.GenerateAndSaveAccountStatementAsync(clientId))
            .ReturnsAsync(new AccountFile { FileId = 1, AccountId = 1, FileContent = new byte[] { } });

        // Act
        await _controller.GenerateAccountStatement(clientId);

        // Assert
        _mockAccountFileService.Verify(s => s.GenerateAndSaveAccountStatementAsync(clientId), Times.Once);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GenerateAccountStatement_WhenServiceThrows_Returns500InternalServerError()
    {
        // Arrange
        int accountId = 1;
        _mockAccountFileService
            .Setup(s => s.GenerateAndSaveAccountStatementAsync(accountId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GenerateAccountStatement(accountId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    #endregion
}
