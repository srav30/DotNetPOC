using Microsoft.AspNetCore.Mvc;
using AccountAPI.Models;
using AccountAPI.Services;

namespace AccountAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    [HttpGet("{clientId}")]
    public async Task<ActionResult<Account>> GetAccount(int clientId)
    {
        var account = await _accountService.GetAccountAsync(clientId);
        if (account == null)
            return NotFound($"Account not found for client {clientId}");
        return Ok(account);
    }

    [HttpPost("withdraw")]
    public async Task<ActionResult<Account>> Withdraw([FromBody] WithdrawalRequest request)
    {
        var account = await _accountService.WithdrawAsync(request.ClientId, request.Amount);
        if (account == null)
        {
            var existingAccount = await _accountService.GetAccountAsync(request.ClientId);
            if (existingAccount == null)
                return NotFound($"Account not found for client {request.ClientId}");
            
            return BadRequest($"Insufficient funds. Available: {existingAccount.Balance}, Requested: {request.Amount}");
        }
        return Ok(account);
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<Account>> Deposit([FromBody] DepositRequest request)
    {
        var account = await _accountService.DepositAsync(request.ClientId, request.Amount);
        if (account == null)
            return NotFound($"Account not found for client {request.ClientId}");
        return Ok(account);

    }

    [HttpPost("verify-funds")]
   public async Task<ActionResult<bool>> VerifyFunds([FromBody] FundVerificationRequest request)
   {
      var hasFunds = await _accountService.VerifyFundsAsync(request.ClientId, request.RequiredAmount);
      return Ok(hasFunds);
   }
}
