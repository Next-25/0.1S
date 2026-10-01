using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace _01S.Model
{
    public enum DocumentType
    {
        Receipt = 0, //Поступление
        Sale = 1 //Продажа

    }

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

        // Тип документа
        public DocumentType Type { get; set; } = DocumentType.Receipt;

        public ObservableCollection<DocumentLine> Lines { get; set; } = [];

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
                Id = Id,
                Number = Number,
                Date = Date,
                Customer = Customer,
                Type = Type
            };

            foreach (var line in Lines)
            {
                clone.Lines.Add(new DocumentLine
                {
                    Id = line.Id,
                    ProductId = line.ProductId,
                    Product = line.Product,
                    Price = line.Price,
                    Quantity = line.Quantity
                });
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

        [ObservableProperty]
        public partial Document? Document { get; set; }

        public int ProductId { get; set; }

        [ObservableProperty]
        public partial Product Product { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Sum))]
        public partial decimal Quantity { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Sum))]
        public partial decimal Price { get; set; }

        public decimal Sum => Quantity * Price;       

        //public void SelectProduct(Product product)
        //{
        //    Product = product;
        //    ProductId = product.Id;
        //    Price = product.Price;
        //}
    }
}
