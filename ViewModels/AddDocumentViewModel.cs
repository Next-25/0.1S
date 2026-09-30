using System.Collections.ObjectModel;
using _01S.Data;
using _01S.Model;
using _01S.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace _01S.ViewModels
{
    public partial class AddDocumentViewModel : ObservableObject
    {
        public static string Title => "Новый документ";

        private readonly ApplicationDbContext _dbContext;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        public partial Document NewDocument { get; set; } = new();

        // Список всех доступных товаров для выбора в ComboBox
        public ObservableCollection<Product> AvailableProducts { get; } = [];

        // Выбранный в ComboBox товар
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLineCommand))]
        public partial Product? SelectedProduct { get; set; }

        // Количество для новой строки
        [ObservableProperty]
        public partial decimal Quantity { get; set; } = 1;

        // Выбранная строка в DataGrid (для удаления)
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveLineCommand))]
        public partial DocumentLine? SelectedLine { get; set; }

        public AddDocumentViewModel(
            ApplicationDbContext context,
            INavigationService navigationService
        )
        {
            _dbContext = context;
            _navigationService = navigationService;

            // Загружаем список товаров из БД
            LoadProducts();
        }

        private void LoadProducts()
        {
            var products = _dbContext.Products.AsNoTracking().ToList();

            AvailableProducts.Clear();
            foreach (var product in products)
            {
                AvailableProducts.Add(product);
            }
        }

        private bool CanAddLine() => SelectedProduct != null && Quantity > 0;

        [RelayCommand(CanExecute = nameof(CanAddLine))]
        private void AddLine()
        {
            if (SelectedProduct is null)
                return;

            // Проверяем, есть ли уже такой товар в документе
            var existingLine = NewDocument.Lines.FirstOrDefault(l =>
                l.ProductId == SelectedProduct.Id
            );

            if (existingLine is not null)
            {
                // Если есть — просто увеличиваем количество
                existingLine.Quantity += Quantity;
            }
            else
            {
                // Если нет — создаём новую строку и фиксируем ТЕКУЩУЮ цену из справочника
                var line = new DocumentLine
                {
                    Product = SelectedProduct,
                    ProductId = SelectedProduct.Id,
                    Price = SelectedProduct.Price, // КОПИРУЕМ ЦЕНУ ТОВАРА
                    Quantity = Quantity,
                };

                NewDocument.Lines.Add(line);
            }

            // Сбрасываем поле ввода количества к значению по умолчанию
            Quantity = 1;
        }

        private bool CanRemoveLine() => SelectedLine is not null;

        [RelayCommand(CanExecute = nameof(CanRemoveLine))]
        private void RemoveLine() => NewDocument.Lines.Remove(SelectedLine!);

        [RelayCommand]
        public async Task SaveDocument()
        {
            if (
                string.IsNullOrWhiteSpace(NewDocument.Customer)
                || string.IsNullOrWhiteSpace(NewDocument.Number)
            )
                return;

            // ВАЖНО: Отвязываем навигационные объекты Product перед сохранением.
            // EF Core сам поймет связь по ProductId и не будет пытаться отслеживать/вставлять сущность Product.
            foreach (var line in NewDocument.Lines)
            {
                line.Product = null;
            }

            _dbContext.Documents.Add(NewDocument);
            await _dbContext.SaveChangesAsync();

            _navigationService.NavigateTo<DocumentsViewModel>();
        }

        [RelayCommand]
        public void CancelDocument()
        {
            _navigationService.NavigateTo<DocumentsViewModel>();
        }
    }
}
