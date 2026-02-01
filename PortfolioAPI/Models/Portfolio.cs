namespace PortfolioAPI.Models;

public class Portfolio
{
    public int PortfolioId { get; set; }
    public int ClientId { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdated { get; set; }
}
