namespace PortfolioAPI.Models;

public class Holding
{
    public int HoldingId { get; set; }
    public int PortfolioId { get; set; }
    public string? Symbol { get; set; }
    public int Quantity { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal TotalValue => Quantity * CurrentPrice;
}
