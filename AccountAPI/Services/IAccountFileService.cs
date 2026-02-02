using AccountAPI.Models;

namespace AccountAPI.Services;

public interface IAccountFileService
{
    Task<AccountFile?> GenerateAndSaveAccountStatementAsync(int clientId);
}
