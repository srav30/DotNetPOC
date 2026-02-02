using System.Text;
using AccountAPI.Models;
using AccountAPI.Repositories;

namespace AccountAPI.Services;

public class AccountFileService : IAccountFileService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountFileRepository _accountFileRepository;
    private readonly ILogger<AccountFileService> _logger;

    public AccountFileService(
        IAccountRepository accountRepository,
        IAccountFileRepository accountFileRepository,
        ILogger<AccountFileService> logger)
    {
        _accountRepository = accountRepository;
        _accountFileRepository = accountFileRepository;
        _logger = logger;
    }

    public async Task<AccountFile?> GenerateAndSaveAccountStatementAsync(int clientId)
    {
        try
        {
            // Retrieve account details
            var account = await _accountRepository.GetAccountByClientIdAsync(clientId);
            if (account == null)
            {
                _logger.LogWarning("Account not found for client {ClientId}", clientId);
                return null;
            }

            // Generate CSV content
            var csvContent = GenerateAccountStatementCsv(account);
            var fileBytes = Encoding.UTF8.GetBytes(csvContent);

            // Create file name with timestamp
            var fileName = $"Account_Statement_{account.ClientId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

            // Save to database
            var accountFile = await _accountFileRepository.SaveAccountFileAsync(
                account.AccountId,
                fileName,
                "csv",
                fileBytes);

            _logger.LogInformation("Generated and saved account statement for client {ClientId}", clientId);
            return accountFile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating account statement for client {ClientId}", clientId);
            throw;
        }
    }

    /// <summary>
    /// Generates CSV content from account data
    /// Format: Header row + 1 data row with account details
    /// </summary>
    private string GenerateAccountStatementCsv(Account account)
    {
        var sb = new StringBuilder();

        // Header row
        sb.AppendLine("AccountId,ClientId,AccountType,Balance,IsActive,CreatedDate");

        // Data row
        sb.AppendLine(
            $"{account.AccountId}," +
            $"{account.ClientId}," +
            $"\"{account.AccountType}\"," +
            $"{account.Balance:F2}," +
            $"{account.IsActive}," +
            $"{account.CreatedDate:yyyy-MM-dd HH:mm:ss}");

        return sb.ToString();
    }
}
