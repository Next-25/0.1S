using System.Collections.ObjectModel;
using _01S.Data;
using _01S.Model;
using _01S.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

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
        private void NavigateAddProduct() => _navigationService.NavigateTo<AddProductViewModel>();

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteProduct))]
        private void NavigateEditProduct()
        {
            _navigationService.NavigateTo<EditProductViewModel, Product>(SelectedProduct!);
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteProduct))]
        private async Task DeleteProduct()
        {
            await _dbContext.Products.Where(p => p.Id == SelectedProduct!.Id).ExecuteDeleteAsync();

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
