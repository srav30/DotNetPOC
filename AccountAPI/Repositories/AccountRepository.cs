using Dapper;
using Npgsql;
using AccountAPI.Models;

namespace AccountAPI.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly string _connectionString;
    private readonly ILogger<AccountRepository> _logger;

    public AccountRepository(IConfiguration configuration, ILogger<AccountRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _logger = logger;
    }

    public async Task<Account?> GetAccountByClientIdAsync(int clientId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
                SELECT account_id as AccountId, 
                       client_id as ClientId, 
                       account_type as AccountType, 
                       balance as Balance, 
                       created_date as CreatedDate, 
                       is_active as IsActive
                FROM accounts 
                WHERE client_id = @ClientId";

            var account = await connection.QueryFirstOrDefaultAsync<Account>(
                sql, 
                new { ClientId = clientId });

            return account;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account for client {ClientId}", clientId);
            throw;
        }
    }

    public async Task<Account?> WithdrawAsync(int clientId, decimal amount)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Check if funds are available
            const string selectSql = "SELECT balance FROM accounts WHERE client_id = @ClientId";
            var currentBalance = await connection.QueryFirstOrDefaultAsync<decimal?>(
                selectSql, 
                new { ClientId = clientId });

            if (currentBalance == null)
            {
                _logger.LogWarning("Account not found for client {ClientId}", clientId);
                return null;
            }

            if (currentBalance < amount)
            {
                _logger.LogWarning("Insufficient funds for client {ClientId}. Available: {Balance}, Requested: {Amount}", 
                    clientId, currentBalance, amount);
                return null;
            }

            // Debit the account
            const string updateSql = @"
                UPDATE accounts 
                SET balance = balance - @Amount 
                WHERE client_id = @ClientId 
                RETURNING account_id as AccountId, 
                          client_id as ClientId, 
                          account_type as AccountType, 
                          balance as Balance, 
                          created_date as CreatedDate, 
                          is_active as IsActive";

            var updatedAccount = await connection.QueryFirstOrDefaultAsync<Account>(
                updateSql,
                new { ClientId = clientId, Amount = amount });

            _logger.LogInformation("Withdrew {Amount} from client {ClientId}. New balance: {Balance}", 
                amount, clientId, updatedAccount?.Balance);

            return updatedAccount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing funds for client {ClientId}", clientId);
            throw;
        }
    }

    public async Task<Account?> DepositAsync(int clientId, decimal amount)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string updateSql = @"
                UPDATE accounts 
                SET balance = balance + @Amount 
                WHERE client_id = @ClientId 
                RETURNING account_id as AccountId, 
                          client_id as ClientId, 
                          account_type as AccountType, 
                          balance as Balance, 
                          created_date as CreatedDate, 
                          is_active as IsActive";

            var updatedAccount = await connection.QueryFirstOrDefaultAsync<Account>(
                updateSql,
                new { ClientId = clientId, Amount = amount });

            if (updatedAccount != null)
            {
                _logger.LogInformation("Deposited {Amount} to client {ClientId}. New balance: {Balance}", 
                    amount, clientId, updatedAccount.Balance);
            }

            return updatedAccount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error depositing funds for client {ClientId}", clientId);
            throw;
        }
    }
}
