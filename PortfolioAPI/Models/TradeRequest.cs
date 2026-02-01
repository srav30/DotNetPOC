namespace PortfolioAPI.Models;

public class TradeRequest
{
    public int ClientId { get; set; }
    public string? Symbol { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalCost => Quantity * Price;
}
