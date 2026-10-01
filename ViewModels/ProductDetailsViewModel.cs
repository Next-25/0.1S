using _01S.Data;
using _01S.Model;
using _01S.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace _01S.ViewModels
{
    public partial class ProductDetailsViewModel : ObservableObject, IInitializable<Product>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly INavigationService _navigationService;

        private int _productId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Title))]
        [NotifyPropertyChangedFor(nameof(IsEditMode))]
        public partial bool IsNew { get; set; } = true;

        public bool IsEditMode => !IsNew;

        public string Title => IsNew ? "Новый товар" : $"Редактирование: {EditableProduct.Name}";

        [ObservableProperty]
        public partial Product EditableProduct { get; set; } = new();

        public ProductDetailsViewModel(
            ApplicationDbContext context,
            INavigationService navigationService
        )
        {
            _dbContext = context;
            _navigationService = navigationService;
        }

        public void Initialize(Product? product)
        {
            if (product is null || product.Id == 0)
            {
                // РЕЖИМ СОЗДАНИЯ
                IsNew = true;
                _productId = 0;
                EditableProduct = new Product();
            }
            else
            {
                // РЕЖИМ РЕДАКТИРОВАНИЯ
                IsNew = false;
                _productId = product.Id;
                EditableProduct = product.Clone();
            }
        }

        [RelayCommand]
        public async Task Save()
        {
            if (string.IsNullOrWhiteSpace(EditableProduct.Name) || EditableProduct.Price <= 0)
                return;

            if (IsNew)
            {
                _dbContext.Products.Add(EditableProduct);
            }
            else
            {
                var existingProduct = await _dbContext.Products.FindAsync(_productId);
                if (existingProduct != null)
                {
                    _dbContext.Entry(existingProduct).CurrentValues.SetValues(EditableProduct);
                }
            }

            await _dbContext.SaveChangesAsync();
            _navigationService.NavigateTo<ProductsViewModel>();
        }

        [RelayCommand]
        public void Cancel()
        {
            _navigationService.NavigateTo<ProductsViewModel>();
        }
    }
}