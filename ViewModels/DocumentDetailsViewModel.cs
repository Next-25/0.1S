using _01S.Data;
using _01S.Model;
using _01S.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace _01S.ViewModels
{
    public partial class DocumentDetailsViewModel : ObservableObject, IInitializable<Document>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly INavigationService _navigationService;

        // Идентификатор редактируемого документа (0 если новый)
        private int _documentId;

        // Флаг, определяющий режим (Создание или Редактирование)
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Title))]
        [NotifyPropertyChangedFor(nameof(IsEditMode))]
        public partial bool IsNew { get; set; } = true;

        public bool IsEditMode => !IsNew;

        // Заголовок окна в зависимости от режима
        public string Title =>
            IsNew ? "Новый документ Поступления" : $"Редактирование документа №{EditableDocument.Number}";

        [ObservableProperty]
        public partial Document EditableDocument { get; set; } = new();

        public ObservableCollection<Product> AvailableProducts { get; } = [];

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLineCommand))]
        public partial Product? SelectedProduct { get; set; }

        [ObservableProperty]
        public partial decimal Quantity { get; set; } = 1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveLineCommand))]
        public partial DocumentLine? SelectedLine { get; set; }

        public DocumentDetailsViewModel(
            ApplicationDbContext context,
            INavigationService navigationService
        )
        {
            _dbContext = context;
            _navigationService = navigationService;

            LoadProducts();
        }

        // Вызывается автоматически при навигации
        public void Initialize(Document? document)
        {
            if (document is null || document.Id == 0)
            {
                // РЕЖИМ СОЗДАНИЯ
                IsNew = true;
                _documentId = 0;
                EditableDocument = new Document
                {
                    Date = DateTime.Now,
                    Number = $"ДОК-{DateTime.Now:MMddHHmm}" // например, генерация номера
                };
            }
            else
            {
                // РЕЖИМ РЕДАКТИРОВАНИЯ
                IsNew = false;
                _documentId = document.Id;
                EditableDocument = document.Clone(); // Клонируем, чтобы не мутировать
            }
        }

        private void LoadProducts()
        {
            var products = _dbContext.Products.AsNoTracking().ToList();
            AvailableProducts.Clear();
            foreach (var p in products)
                AvailableProducts.Add(p);
        }

        private bool CanAddLine() => SelectedProduct != null && Quantity > 0;

        [RelayCommand(CanExecute = nameof(CanAddLine))]
        private void AddLine()
        {
            if (SelectedProduct is null)
                return;

            var existingLine = EditableDocument.Lines.FirstOrDefault(l =>
                l.ProductId == SelectedProduct.Id
            );

            if (existingLine is not null)
            {
                existingLine.Quantity += Quantity;
            }
            else
            {
                EditableDocument.Lines.Add(
                    new DocumentLine
                    {
                        Product = SelectedProduct,
                        ProductId = SelectedProduct.Id,
                        Price = SelectedProduct.Price,
                        Quantity = Quantity,
                    }
                );
            }

            Quantity = 1;
        }

        private bool CanRemoveLine() => SelectedLine is not null;

        [RelayCommand(CanExecute = nameof(CanRemoveLine))]
        private void RemoveLine() => EditableDocument.Lines.Remove(SelectedLine!);

        [RelayCommand]
        public async Task Save()
        {
            if (
                string.IsNullOrWhiteSpace(EditableDocument.Customer)
                || string.IsNullOrWhiteSpace(EditableDocument.Number)
            )
                return;

            if (IsNew)
            {
                // Логика СОХРАНЕНИЯ НОВОГО ДОКУМЕНТА
                foreach (var line in EditableDocument.Lines)
                {
                    line.Product = null; // Избавляемся от tracking конфликтов
                }

                _dbContext.Documents.Add(EditableDocument);
            }
            else
            {
                // Логика ОБНОВЛЕНИЯ СУЩЕСТВУЮЩЕГО (наш дифф алгоритм)
                var dbDocument = await _dbContext
                    .Documents.Include(d => d.Lines)
                    .FirstOrDefaultAsync(d => d.Id == _documentId);

                if (dbDocument is null)
                    return;

                dbDocument.Number = EditableDocument.Number;
                dbDocument.Date = EditableDocument.Date;
                dbDocument.Customer = EditableDocument.Customer;

                // Удаляем убранные строки
                var linesToRemove = dbDocument
                    .Lines.Where(dbLine =>
                        !EditableDocument.Lines.Any(eLine => eLine.Id == dbLine.Id && eLine.Id != 0)
                    )
                    .ToList();

                foreach (var line in linesToRemove)
                {
                    _dbContext.DocumentLines.Remove(line);
                }

                // Добавляем новые и обновляем существующие
                foreach (var editLine in EditableDocument.Lines)
                {
                    if (editLine.Id == 0)
                    {
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
                        var dbLine = dbDocument.Lines.FirstOrDefault(l => l.Id == editLine.Id);
                        if (dbLine != null)
                        {
                            dbLine.Quantity = editLine.Quantity;
                            dbLine.Price = editLine.Price;
                            dbLine.ProductId = editLine.ProductId;
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
            _navigationService.NavigateTo<DocumentsViewModel>();
        }

        [RelayCommand]
        public void Cancel()
        {
            _navigationService.NavigateTo<DocumentsViewModel>();
        }
    }
}
