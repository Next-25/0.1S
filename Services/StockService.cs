using _01S.Data;
using _01S.Model;
using Microsoft.EntityFrameworkCore;

namespace _01S.Services
{
    /// <summary>
    /// Пересчитывает складские остатки на основе документов поступления.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных приложения.</param>
    public class StockService(ApplicationDbContext dbContext) : IStockService
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        /// <summary>
        /// Пересчитывает остаток товара по всем документам поступления и сохраняет результат.
        /// </summary>
        /// <param name="productId">Идентификатор товара.</param>
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
    }
}