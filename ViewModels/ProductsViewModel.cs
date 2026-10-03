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
        public ObservableCollection<ProductWithStock> Products { get; } = [];

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NavigateEditProductCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteProductCommand))]
        public partial ProductWithStock? SelectedProduct { get; set; }

        public static string Title => "Товары";

        /// <summary>
        /// Создаёт модель списка товаров и загружает данные из базы.
        /// </summary>
        /// <param name="dbContext">Контекст базы данных приложения.</param>
        /// <param name="navigationService">Сервис навигации.</param>
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

        /// <summary>
        /// Загружает товары и их остатки из базы данных.
        /// </summary>
        private void LoadProducts()
        {
            var items = (from p in _dbContext.Products.AsNoTracking()
                         join s in _dbContext.Stocks.AsNoTracking() on p.Id equals s.ProductId into stockGroup
                         from stock in stockGroup.DefaultIfEmpty()
                         select new ProductWithStock
                         {
                             Product = p,
                             StockQuantity = stock != null ? stock.Quantity : 0
                         }).ToList();

            Products.Clear();
            foreach (var item in items)
            {
                Products.Add(item);
            }
        }

        /// <summary>
        /// Открывает форму создания товара.
        /// </summary>
        [RelayCommand]
        private void NavigateCreateProduct()
        {
            _navigationService.NavigateTo<ProductDetailsViewModel, Product?>(null);
        }

        /// <summary>
        /// Возвращает, можно ли редактировать или удалить выбранный товар.
        /// </summary>
        /// <returns><see langword="true"/>, если товар выбран.</returns>
        private bool CanEditOrDeleteProduct() => SelectedProduct is not null;

        /// <summary>
        /// Открывает форму редактирования выбранного товара.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditOrDeleteProduct))]
        private void NavigateEditProduct()
        {
            if (SelectedProduct is null) return;
            // Передаем внутренний объект Product в окно редактирования
            _navigationService.NavigateTo<ProductDetailsViewModel, Product>(SelectedProduct.Product);
        }

        /// <summary>
        /// Удаляет выбранный товар, если он не используется в документах.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditOrDeleteProduct))]
        private async Task DeleteProduct()
        {
            if (SelectedProduct is null) return;

            var productId = SelectedProduct.Product.Id;

            bool isUsedInDocuments = await _dbContext.DocumentLines
                .AnyAsync(l => l.ProductId == productId);

            if (isUsedInDocuments)
            {
                System.Windows.MessageBox.Show(
                    $"Невозможно удалить товар \"{SelectedProduct.Product.Name}\", так как он используется в документах!",
                    "Ошибка удаления",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning
                );
                return;
            }

            await _dbContext.Products
                .Where(p => p.Id == productId)
                .ExecuteDeleteAsync();

            LoadProducts();
        }

    }
}
