using AccountAPI.Models;
using AccountAPI.Repositories;

namespace AccountAPI.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountService> _logger;

    public AccountService(IAccountRepository accountRepository, ILogger<AccountService> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<Account?> GetAccountAsync(int clientId)
    {
        return await _accountRepository.GetAccountByClientIdAsync(clientId);
    }

    public async Task<Account?> WithdrawAsync(int clientId, decimal amount)
    {
        return await _accountRepository.WithdrawAsync(clientId, amount);
    }

    public async Task<Account?> DepositAsync(int clientId, decimal amount)
    {
        return await _accountRepository.DepositAsync(clientId, amount);
    }
}
