using AccountAPI.Models;

namespace AccountAPI.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetAccountByClientIdAsync(int clientId);
    Task<bool> VerifyFundsAsync(int clientId, decimal requiredAmount);
    Task<Account?> WithdrawAsync(int clientId, decimal amount);
    Task<Account?> DepositAsync(int clientId, decimal amount);
}
