namespace _01S.Model
{
    public class ProductWithStock
    {
        public Product Product { get; set; } = null!;
        public decimal StockQuantity { get; set; }
    }
}
