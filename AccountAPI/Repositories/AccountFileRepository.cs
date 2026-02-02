using Dapper;
using Npgsql;
using AccountAPI.Models;

namespace AccountAPI.Repositories;

public class AccountFileRepository : IAccountFileRepository
{
    private readonly string _connectionString;
    private readonly ILogger<AccountFileRepository> _logger;

    public AccountFileRepository(IConfiguration configuration, ILogger<AccountFileRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
        _logger = logger;
    }

    public async Task<AccountFile?> SaveAccountFileAsync(int accountId, string fileName, string fileType, byte[] fileContent)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                INSERT INTO account_files (account_id, file_name, file_type, file_content, created_date)
                VALUES (@AccountId, @FileName, @FileType, @FileContent, @CreatedDate)
                RETURNING file_id as FileId, 
                          account_id as AccountId, 
                          file_name as FileName, 
                          file_type as FileType, 
                          file_content as FileContent, 
                          created_date as CreatedDate";

            var accountFile = await connection.QueryFirstOrDefaultAsync<AccountFile>(
                sql,
                new
                {
                    AccountId = accountId,
                    FileName = fileName,
                    FileType = fileType,
                    FileContent = fileContent,
                    CreatedDate = DateTime.UtcNow
                });

            _logger.LogInformation("Saved account file {FileName} for account {AccountId}", fileName, accountId);
            return accountFile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving account file for account {AccountId}", accountId);
            throw;
        }
    }
}
