using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace _01S.Model
{
    public enum DocumentType
    {
        Receipt = 0, //Поступление
        Sale = 1, //Продажа
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

        /// <summary>
        /// Создаёт документ и подписывается на изменения его строк.
        /// </summary>
        public Document()
        {
            AttachCollectionEvents();
        }

        /// <summary>
        /// Подписывается на изменения коллекции строк документа.
        /// </summary>
        private void AttachCollectionEvents()
        {
            Lines.CollectionChanged += OnLinesCollectionChanged;
        }

        /// <summary>
        /// Выполняет глубокое клонирование документа и всех его строк.
        /// </summary>
        /// <returns>Копия документа с копиями его строк.</returns>
        public Document Clone()
        {
            var clone = new Document
            {
                Id = Id,
                Number = Number,
                Date = Date,
                Customer = Customer,
                Type = Type,
            };

            foreach (var line in Lines)
            {
                clone.Lines.Add(
                    new DocumentLine
                    {
                        Id = line.Id,
                        ProductId = line.ProductId,
                        Product = line.Product,
                        Price = line.Price,
                        Quantity = line.Quantity,
                    }
                );
            }

            return clone;
        }

        /// <summary>
        /// Обновляет подписки на строки и уведомляет об изменении суммы документа.
        /// </summary>
        /// <param name="sender">Коллекция, в которой изменились строки.</param>
        /// <param name="e">Описание добавленных и удалённых элементов.</param>
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

        /// <summary>
        /// Уведомляет об изменении суммы документа при изменении суммы строки.
        /// </summary>
        /// <param name="sender">Изменённая строка документа.</param>
        /// <param name="e">Описание изменившегося свойства строки.</param>
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
    }
}
