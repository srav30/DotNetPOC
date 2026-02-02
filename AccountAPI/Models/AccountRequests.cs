namespace AccountAPI.Models;

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
