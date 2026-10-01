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

        // Коллекция, к которой будет привязываться View
        public ObservableCollection<Document> Documents { get; } = [];

        [ObservableProperty]
        public partial Document? SelectedDocument { get; set; }

        public static string Title => "Документы Поступления";

        public DocumentsViewModel(
            ApplicationDbContext dbContext,
            INavigationService navigationService
        )
        {
            _dbContext = dbContext;
            _navigationService = navigationService;

            // Загружаем документы из БД при создании ViewModel
            LoadDocuments();
        }

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

        [RelayCommand]
        private void NavigateCreateDocument()
        {
            _navigationService.NavigateTo<DocumentDetailsViewModel, Document?>(null);
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteDocument))]
        private void NavigateEditDocument()
        {
            // Передаем существующий объект
            _navigationService.NavigateTo<DocumentDetailsViewModel, Document>(SelectedDocument!);
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteDocument))]
        private async Task DeleteDocument()
        {
            await _dbContext
                .Documents.Where(p => p.Id == SelectedDocument!.Id)
                .ExecuteDeleteAsync();

            LoadDocuments();
        }

        private bool CanEditOrDeleteDocument() => SelectedDocument is not null;

        // При изменении SelectedProduct уведомляем команды:
        partial void OnSelectedDocumentChanged(Document? value)
        {
            NavigateEditDocumentCommand.NotifyCanExecuteChanged();
            DeleteDocumentCommand.NotifyCanExecuteChanged();
        }
    }
}
