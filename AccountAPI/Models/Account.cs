namespace AccountAPI.Models;

public class Account
{
    public int AccountId { get; set; }
    public int ClientId { get; set; }
    public string? AccountType { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}
