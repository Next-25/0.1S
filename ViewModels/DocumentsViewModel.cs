using _01S.Data;
using _01S.Model;
using _01S.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace _01S.ViewModels
{
    public partial class DocumentsViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IStockService _stockService;
            
        // Коллекция, к которой будет привязываться View
        public ObservableCollection<Document> Documents { get; } = [];

        [ObservableProperty]
        public partial Document? SelectedDocument { get; set; }

        public static string Title => "Документы Поступления";

        /// <summary>
        /// Создаёт модель списка документов и загружает данные из базы.
        /// </summary>
        /// <param name="dbContext">Контекст базы данных приложения.</param>
        /// <param name="navigationService">Сервис навигации.</param>
        /// <param name="stockService">Сервис пересчёта остатков товаров.</param>
        public DocumentsViewModel(
            ApplicationDbContext dbContext,
            INavigationService navigationService,
            IStockService stockService
        )
        {
            _dbContext = dbContext;
            _navigationService = navigationService;
            _stockService = stockService;

            // Загружаем документы из БД при создании ViewModel
            LoadDocuments();
        }

        /// <summary>
        /// Загружает документы вместе с их строками и товарами.
        /// </summary>
        private void LoadDocuments()
        {
            var items = _dbContext
                .Documents.Include(d => d.Lines)
                    .ThenInclude(l => l.Product)
                .AsNoTracking()
                .ToList();

            Documents.Clear();
            foreach (var item in items)
            {
                Documents.Add(item);
            }
        }

        /// <summary>
        /// Открывает форму создания документа.
        /// </summary>
        [RelayCommand]
        private void NavigateCreateDocument()
        {
            _navigationService.NavigateTo<DocumentDetailsViewModel, Document?>(null);
        }

        /// <summary>
        /// Открывает форму редактирования выбранного документа.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditOrDeleteDocument))]
        private void NavigateEditDocument()
        {
            // Передаем существующий объект
            _navigationService.NavigateTo<DocumentDetailsViewModel, Document>(SelectedDocument!);
        }

        /// <summary>
        /// Удаляет выбранный документ и пересчитывает остатки затронутых товаров.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditOrDeleteDocument))]
        private async Task DeleteDocument()
        {
            if (SelectedDocument is null) return;

            // Запоминаем товары из удаляемого документа
            var productIds = SelectedDocument.Lines
                .Select(l => l.ProductId)
                .Distinct()
                .ToList();

            await _dbContext.Documents
                .Where(p => p.Id == SelectedDocument.Id)
                .ExecuteDeleteAsync();

            // Пересчитываем остатки
            foreach (var productId in productIds)
            {
                await _stockService.RecalculateProductStockAsync(productId);
            }

            LoadDocuments();
        }

        /// <summary>
        /// Возвращает, можно ли редактировать или удалить выбранный документ.
        /// </summary>
        /// <returns><see langword="true"/>, если документ выбран.</returns>
        private bool CanEditOrDeleteDocument() => SelectedDocument is not null;

        /// <summary>
        /// Обновляет доступность команд редактирования и удаления после смены выбора.
        /// </summary>
        /// <param name="value">Новый выбранный документ.</param>
        partial void OnSelectedDocumentChanged(Document? value)
        {
            NavigateEditDocumentCommand.NotifyCanExecuteChanged();
            DeleteDocumentCommand.NotifyCanExecuteChanged();
        }
    }
}
