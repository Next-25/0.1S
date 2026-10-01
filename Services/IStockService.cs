namespace _01S.Services
{
    public interface IStockService
    {
        Task RecalculateProductStockAsync(int productId);
        Task RecalculateAllStocksAsync();
    }
}
