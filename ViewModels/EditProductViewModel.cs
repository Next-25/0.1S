using _01S.Data;
using _01S.Model;
using _01S.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace _01S.ViewModels
{
    public partial class EditProductViewModel(
        ApplicationDbContext context,
        INavigationService navigationService
    ) : ObservableObject, IInitializable<Product>
    {
        public static string Title => "Редактирование";

        private readonly ApplicationDbContext _dbContext = context;
        private readonly INavigationService _navigationService = navigationService;

        // Оригинальный Id для обновления в БД
        private int _originalProductId;

        [ObservableProperty]
        public partial Product EditableProduct { get; set; } = new();

        public void Initialize(Product product)
        {
            _originalProductId = product.Id;

            EditableProduct = product.Clone();
        }

        [RelayCommand]
        public async Task SaveChangeProduct()
        {
            if (string.IsNullOrWhiteSpace(EditableProduct.Name) || EditableProduct.Price <= 0)
                return;

            // Находим сущность в контексте БД и обновляем
            var existingProduct = await _dbContext.Products.FindAsync(_originalProductId);
            if (existingProduct != null)
            {
                _dbContext.Entry(existingProduct).CurrentValues.SetValues(EditableProduct);
                await _dbContext.SaveChangesAsync();
            }

            _navigationService.NavigateTo<ProductsViewModel>();
        }

        [RelayCommand]
        public void CancelProduct()
        {
            _navigationService.NavigateTo<ProductsViewModel>();
        }
    }
}
