using _01S.Data;
using _01S.Model;
using Microsoft.EntityFrameworkCore;

namespace _01S.Services
{
    public class StockService(ApplicationDbContext dbContext) : IStockService
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task RecalculateProductStockAsync(int productId)
        {
            // EF Core теперь корректно сгенерирует INNER JOIN по единственному DocumentId
            var totalReceiptQuantity = await _dbContext.DocumentLines
                .Where(l => l.ProductId == productId && l.Document!.Type == DocumentType.Receipt)
                .SumAsync(l => (decimal?)l.Quantity) ?? 0m;

            var stock = await _dbContext.Stocks
                .FirstOrDefaultAsync(s => s.ProductId == productId);

            if (stock is null)
            {
                stock = new Stock
                {
                    ProductId = productId,
                    Quantity = totalReceiptQuantity
                };
                _dbContext.Stocks.Add(stock);
            }
            else
            {
                stock.Quantity = totalReceiptQuantity;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task RecalculateAllStocksAsync()
        {
            var products = await _dbContext.Products
                .Select(p => p.Id)
                .ToListAsync();

            foreach (var productId in products)
            {
                await RecalculateProductStockAsync(productId);
            }
        }
    }
}