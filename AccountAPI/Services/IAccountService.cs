using AccountAPI.Models;

namespace AccountAPI.Services;

public interface IAccountService
{
    Task<Account?> GetAccountAsync(int clientId);
    Task<Account?> WithdrawAsync(int clientId, decimal amount);
    Task<Account?> DepositAsync(int clientId, decimal amount);
}
