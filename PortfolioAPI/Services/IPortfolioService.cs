using PortfolioAPI.Models;

namespace PortfolioAPI.Services;

public interface IPortfolioService
{
    Task<Portfolio?> GetPortfolioAsync(int clientId);
    Task<IEnumerable<Holding>> GetHoldingsAsync(int clientId);
    Task<TradeResponse> BuyStockAsync(TradeRequest request);
}
