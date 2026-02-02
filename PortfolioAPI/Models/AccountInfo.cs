namespace PortfolioAPI.Models;

public class AccountInfo
{
    public int AccountId { get; set; }
    public int ClientId { get; set; }
    public string? AccountType { get; set; }
    public decimal Balance { get; set; }
}
