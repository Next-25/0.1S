using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace _01S.Model
{
    public partial class Document : ObservableObject
    {
        [Key]
        public int Id { get; set; }

        [ObservableProperty]
        public partial string Number { get; set; } = string.Empty;

        [ObservableProperty]
        public partial DateTime Date { get; set; } = DateTime.Now;

        [ObservableProperty]
        public partial string Customer { get; set; } = string.Empty;

        public ObservableCollection<DocumentLine> Lines { get; } = [];

        public decimal SumOfDocument => Lines.Sum(l => l.Sum);

        public Document()
        {
            AttachCollectionEvents();
        }

        private void AttachCollectionEvents()
        {
            Lines.CollectionChanged += OnLinesCollectionChanged;
        }

        /// <summary>
        /// Выполняет глубокое клонирование документа и всех его строк.
        /// </summary>
        public Document Clone()
        {
            var clone = new Document
            {
                Id = this.Id,
                Number = this.Number,
                Date = this.Date,
                Customer = this.Customer,
            };

            foreach (var line in this.Lines)
            {
                clone.Lines.Add(line.Clone());
            }

            return clone;
        }

        private void OnLinesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (DocumentLine line in e.NewItems)
                {
                    line.PropertyChanged += OnLinePropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (DocumentLine line in e.OldItems)
                {
                    line.PropertyChanged -= OnLinePropertyChanged;
                }
            }

            OnPropertyChanged(nameof(SumOfDocument));
        }

        private void OnLinePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DocumentLine.Sum))
            {
                OnPropertyChanged(nameof(SumOfDocument));
            }
        }
    }

    public partial class DocumentLine : ObservableObject
    {
        [Key]
        public int Id { get; set; }

        public int DocumentId { get; set; }

        public int? ProductId { get; set; }

        [ObservableProperty]
        public partial Product? Product { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Sum))]
        public partial decimal Quantity { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Sum))]
        public partial decimal Price { get; set; }

        public decimal Sum => Quantity * Price;

        public void SelectProduct(Product product)
        {
            Product = product;
            ProductId = product.Id;
            Price = product.Price;
        }

        /// <summary>
        /// Клонирует строку документа, сохраняя первичные ключи для EF Core.
        /// </summary>
        public DocumentLine Clone()
        {
            return new DocumentLine
            {
                Id = this.Id,
                DocumentId = this.DocumentId,
                ProductId = this.ProductId,
                Product = this.Product, // Ссылка на товар из справочника
                Quantity = this.Quantity,
                Price = this.Price,
            };
        }
    }
}
