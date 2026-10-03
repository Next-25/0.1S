namespace _01S.Services
{
    public interface IStockService
    {
        /// <summary>
        /// Пересчитывает и сохраняет остаток указанного товара.
        /// </summary>
        /// <param name="productId">Идентификатор товара.</param>
        Task RecalculateProductStockAsync(int productId);
    }
}
