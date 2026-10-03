using System.ComponentModel.DataAnnotations;

namespace _01S.Model
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        /// <summary>
        /// Создаёт копию товара.
        /// </summary>
        /// <returns>Новый экземпляр с текущими значениями свойств товара.</returns>
        public Product Clone() => (Product)this.MemberwiseClone();
    }
}
