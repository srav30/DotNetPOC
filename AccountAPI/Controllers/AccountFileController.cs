using Microsoft.AspNetCore.Mvc;
using AccountAPI.Services;

namespace AccountAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountFileController : ControllerBase
{
    private readonly IAccountFileService _accountFileService;
    private readonly ILogger<AccountFileController> _logger;

    public AccountFileController(IAccountFileService accountFileService, ILogger<AccountFileController> logger)
    {
        _accountFileService = accountFileService;
        _logger = logger;
    }

    [HttpGet("{clientId}/statement")]
    public async Task<IActionResult> GenerateAccountStatement(int clientId)
    {
        try
        {
            var accountFile = await _accountFileService.GenerateAndSaveAccountStatementAsync(clientId);
            
            if (accountFile == null)
                return NotFound(new { message = $"Account not found for client {clientId}" });

            // Return the CSV file for download
            return File(
                accountFile.FileContent,
                "text/csv",
                accountFile.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating account statement for client {ClientId}", clientId);
            return StatusCode(500, new { message = "Error generating account statement", error = ex.Message });
        }
    }
}
