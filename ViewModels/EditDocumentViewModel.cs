using System.Collections.ObjectModel;
using _01S.Data;
using _01S.Model;
using _01S.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace _01S.ViewModels
{
    public partial class EditDocumentViewModel : ObservableObject, IInitializable<Document>
    {
        public static string Title => "Редактирование документа";

        private readonly ApplicationDbContext _dbContext;
        private readonly INavigationService _navigationService;

        // Оригинальный Id для обновления в БД
        private int _originalDocumentId;

        [ObservableProperty]
        public partial Document EditableDocument { get; set; } = new();

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

        public EditDocumentViewModel(
            ApplicationDbContext context,
            INavigationService navigationService
        )
        {
            _dbContext = context;
            _navigationService = navigationService;

            // Загружаем список товаров из БД
            LoadProducts();
        }

        public void Initialize(Document document)
        {
            _originalDocumentId = document.Id;

            // Используем глубинное клонирование, чтобы не мутировать оригинал раньше времени
            EditableDocument = document.Clone();
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
            var existingLine = EditableDocument.Lines.FirstOrDefault(l =>
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

                EditableDocument.Lines.Add(line);
            }

            // Сбрасываем поле ввода количества к значению по умолчанию
            Quantity = 1;
        }

        private bool CanRemoveLine() => SelectedLine is not null;

        [RelayCommand(CanExecute = nameof(CanRemoveLine))]
        private void RemoveLine() => EditableDocument.Lines.Remove(SelectedLine!);

        [RelayCommand]
        public async Task SaveDocument()
        {
            if (
                string.IsNullOrWhiteSpace(EditableDocument.Customer)
                || string.IsNullOrWhiteSpace(EditableDocument.Number)
            )
                return;

            // 1. Загружаем документ из БД ВМЕСТЕ со строками (Lines)
            var dbDocument = await _dbContext
                .Documents.Include(d => d.Lines)
                .FirstOrDefaultAsync(d => d.Id == _originalDocumentId);

            if (dbDocument is null)
                return;

            // 2. Обновляем скалярные поля шапки
            dbDocument.Number = EditableDocument.Number;
            dbDocument.Date = EditableDocument.Date;
            dbDocument.Customer = EditableDocument.Customer;

            // 3. Синхронизируем коллекцию Lines:

            // А) Удаляем из БД те строки, которых больше нет в отредактированном документе
            var linesToRemove = dbDocument
                .Lines.Where(dbLine =>
                    !EditableDocument.Lines.Any(eLine => eLine.Id == dbLine.Id && eLine.Id != 0)
                )
                .ToList();

            foreach (var line in linesToRemove)
            {
                _dbContext.DocumentLines.Remove(line);
            }

            // Б) Обновляем существующие строки и добавляем новые
            foreach (var editLine in EditableDocument.Lines)
            {
                if (editLine.Id == 0)
                {
                    // Новая строка
                    dbDocument.Lines.Add(
                        new DocumentLine
                        {
                            ProductId = editLine.ProductId,
                            Price = editLine.Price,
                            Quantity = editLine.Quantity,
                        }
                    );
                }
                else
                {
                    // Существующая строка — обновляем количество и цену
                    var dbLine = dbDocument.Lines.FirstOrDefault(l => l.Id == editLine.Id);
                    if (dbLine is not null)
                    {
                        dbLine.Quantity = editLine.Quantity;
                        dbLine.Price = editLine.Price;
                        dbLine.ProductId = editLine.ProductId;
                    }
                }
            }

            // 4. Фиксируем изменения в транзакции
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
