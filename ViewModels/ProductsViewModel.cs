using _01S.Data;
using _01S.Model;
using _01S.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace _01S.ViewModels
{
    public partial class ProductsViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly ApplicationDbContext _dbContext;

        // Коллекция, к которой будет привязываться View
        public ObservableCollection<Product> Products { get; } = [];

        [ObservableProperty]
        public partial Product? SelectedProduct { get; set; }

        public static string Title => "Товары";

        public ProductsViewModel(
            ApplicationDbContext dbContext,
            INavigationService navigationService
        )
        {
            _dbContext = dbContext;
            _navigationService = navigationService;

            // Загружаем товары из БД при создании ViewModel
            LoadProducts();
        }

        private void LoadProducts()
        {
            var items = _dbContext.Products.AsNoTracking().ToList();

            Products.Clear();
            foreach (var item in items)
            {
                Products.Add(item);
            }
        }

        [RelayCommand]
        private void NavigateCreateProduct()
        {
            _navigationService.NavigateTo<ProductDetailsViewModel, Product?>(null);
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteProduct))]
        private void NavigateEditProduct()
        {
            if (SelectedProduct is null) return;
            _navigationService.NavigateTo<ProductDetailsViewModel, Product>(SelectedProduct);
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteProduct))]
        private async Task DeleteProduct()
        {
            if (SelectedProduct is null) return;

            // 1. Проверяем, ссылается ли хоть одна строка документа на этот товар
            bool isUsedInDocuments = await _dbContext.DocumentLines
                .AnyAsync(l => l.ProductId == SelectedProduct.Id);

            if (isUsedInDocuments)
            {
                // Покажем пользователю плашку/MessageBox
                System.Windows.MessageBox.Show(
                    $"Невозможно удалить товар \"{SelectedProduct.Name}\", так как он используется в документах!",
                    "Ошибка удаления",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning
                );
                return;
            }

            // 2. Если связей нет — безопасно удаляем
            await _dbContext.Products
                .Where(p => p.Id == SelectedProduct.Id)
                .ExecuteDeleteAsync();

            LoadProducts();
        }

        private bool CanEditOrDeleteProduct() => SelectedProduct is not null;

        // При изменении SelectedProduct уведомляем команды:
        partial void OnSelectedProductChanged(Product? value)
        {
            NavigateEditProductCommand.NotifyCanExecuteChanged();
            DeleteProductCommand.NotifyCanExecuteChanged();
        }
    }
}
