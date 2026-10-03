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
        private readonly IStockService _stockService;

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

        /// <summary>
        /// Создаёт модель формы документа и загружает доступные товары.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        /// <param name="navigationService">Сервис навигации.</param>
        /// <param name="stockService">Сервис пересчёта остатков товаров.</param>
        public DocumentDetailsViewModel(
            ApplicationDbContext context,
            INavigationService navigationService,
            IStockService stockService
        )
        {
            _dbContext = context;
            _navigationService = navigationService;
            _stockService = stockService;

            LoadProducts();
        }

        /// <summary>
        /// Подготавливает форму для создания документа или редактирования его копии.
        /// </summary>
        /// <param name="document">Документ для редактирования либо <see langword="null"/> для нового.</param>
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
                    Number = $"ДОК-{DateTime.Now:MMddHHmm}", // например, генерация номера
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

        /// <summary>
        /// Загружает товары, доступные для добавления в документ.
        /// </summary>
        private void LoadProducts()
        {
            var products = _dbContext.Products.AsNoTracking().ToList();
            AvailableProducts.Clear();
            foreach (var p in products)
                AvailableProducts.Add(p);
        }

        /// <summary>
        /// Возвращает, можно ли добавить выбранный товар с заданным количеством.
        /// </summary>
        /// <returns><see langword="true"/>, если товар выбран и количество положительно.</returns>
        private bool CanAddLine() => SelectedProduct != null && Quantity > 0;

        /// <summary>
        /// Добавляет выбранный товар в документ или увеличивает количество существующей строки.
        /// </summary>
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

        /// <summary>
        /// Возвращает, можно ли удалить выбранную строку документа.
        /// </summary>
        /// <returns><see langword="true"/>, если строка выбрана.</returns>
        private bool CanRemoveLine() => SelectedLine is not null;

        /// <summary>
        /// Удаляет выбранную строку документа.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRemoveLine))]
        private void RemoveLine() => EditableDocument.Lines.Remove(SelectedLine!);

        /// <summary>
        /// Проверяет и сохраняет документ, пересчитывает затронутые остатки и возвращается к списку.
        /// </summary>
        [RelayCommand]
        public async Task Save()
        {
            if (string.IsNullOrWhiteSpace(EditableDocument.Customer) ||
                string.IsNullOrWhiteSpace(EditableDocument.Number))
                return;

            // Фиксируем список ID товаров, которые фигурируют в этом документе
            var affectedProductIds = EditableDocument.Lines
                    .Select(l => l.ProductId)                    
                    .Distinct()
                    .ToList();

            if (IsNew)
            {
                foreach (var line in EditableDocument.Lines)
                {
                    line.Product = null;
                }

                _dbContext.Documents.Add(EditableDocument);
            }
            else
            {
                var dbDocument = await _dbContext.Documents
                    .Include(d => d.Lines)
                    .FirstOrDefaultAsync(d => d.Id == _documentId);

                if (dbDocument is null) return;

                // Также добавляем товары, которые БЫЛИ в документе до редактирования
                affectedProductIds.AddRange(dbDocument.Lines.Select(l => l.ProductId));
                affectedProductIds = [.. affectedProductIds.Distinct()];

                dbDocument.Number = EditableDocument.Number;
                dbDocument.Date = EditableDocument.Date;
                dbDocument.Customer = EditableDocument.Customer;
                dbDocument.Type = EditableDocument.Type;

                // Удаляем убранные строки
                var linesToRemove = dbDocument.Lines
                    .Where(dbLine => !EditableDocument.Lines.Any(eLine => eLine.Id == dbLine.Id && eLine.Id != 0))
                    .ToList();

                foreach (var line in linesToRemove)
                {
                    _dbContext.DocumentLines.Remove(line);
                }

                // Обновляем / Добавляем строки
                foreach (var editLine in EditableDocument.Lines)
                {
                    if (editLine.Id == 0)
                    {
                        dbDocument.Lines.Add(new DocumentLine
                        {
                            ProductId = editLine.ProductId,
                            Price = editLine.Price,
                            Quantity = editLine.Quantity,
                        });
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

            // Пересчитываем остатки для всех затронутых товаров
            foreach (var productId in affectedProductIds)
            {
                await _stockService.RecalculateProductStockAsync(productId);
            }

            _navigationService.NavigateTo<DocumentsViewModel>();
        }

        /// <summary>
        /// Отменяет редактирование и возвращается к списку документов.
        /// </summary>
        [RelayCommand]
        public void Cancel()
        {
            _navigationService.NavigateTo<DocumentsViewModel>();
        }
    }
}
