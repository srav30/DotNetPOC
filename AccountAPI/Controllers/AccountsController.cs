using Microsoft.AspNetCore.Mvc;
using AccountAPI.Models;
using AccountAPI.Repositories;

namespace AccountAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IAccountRepository accountRepository, ILogger<AccountsController> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    [HttpGet("{clientId}")]
    public async Task<ActionResult<Account>> GetAccount(int clientId)
    {
        var account = await _accountRepository.GetAccountByClientIdAsync(clientId);
        if (account == null)
            return NotFound($"Account not found for client {clientId}");
        return Ok(account);
    }

    [HttpPost("verify-funds")]
    public async Task<ActionResult<bool>> VerifyFunds([FromBody] FundVerificationRequest request)
    {
        var hasFunds = await _accountRepository.VerifyFundsAsync(request.ClientId, request.RequiredAmount);
        return Ok(hasFunds);
    }

    [HttpPost("withdraw")]
    public async Task<ActionResult<Account>> Withdraw([FromBody] WithdrawalRequest request)
    {
        var account = await _accountRepository.WithdrawAsync(request.ClientId, request.Amount);
        if (account == null)
        {
            var existingAccount = await _accountRepository.GetAccountByClientIdAsync(request.ClientId);
            if (existingAccount == null)
                return NotFound($"Account not found for client {request.ClientId}");
            
            return BadRequest($"Insufficient funds. Available: {existingAccount.Balance}, Requested: {request.Amount}");
        }
        return Ok(account);
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<Account>> Deposit([FromBody] DepositRequest request)
    {
        var account = await _accountRepository.DepositAsync(request.ClientId, request.Amount);
        if (account == null)
            return NotFound($"Account not found for client {request.ClientId}");
        return Ok(account);
    }
}

public class FundVerificationRequest
{
    public int ClientId { get; set; }
    public decimal RequiredAmount { get; set; }
}

public class WithdrawalRequest
{
    public int ClientId { get; set; }
    public decimal Amount { get; set; }
}

public class DepositRequest
{
    public int ClientId { get; set; }
    public decimal Amount { get; set; }
}
