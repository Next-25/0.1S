using System.ComponentModel.DataAnnotations;

namespace _01S.Model
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        //Накопленное количество остатка
        public decimal Quantity { get; set; }
    }
}
