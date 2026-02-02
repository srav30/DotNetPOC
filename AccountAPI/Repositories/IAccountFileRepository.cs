using AccountAPI.Models;

namespace AccountAPI.Repositories;

public interface IAccountFileRepository
{
    Task<AccountFile?> SaveAccountFileAsync(int accountId, string fileName, string fileType, byte[] fileContent);
}
