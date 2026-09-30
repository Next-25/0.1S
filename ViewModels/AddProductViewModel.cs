using _01S.Data;
using _01S.Model;
using _01S.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace _01S.ViewModels
{
    public partial class AddProductViewModel(
        ApplicationDbContext context,
        INavigationService navigationService
    ) : ObservableObject
    {
        public static string Title => "Новый товар";

        private readonly ApplicationDbContext _dbContext = context;
        private readonly INavigationService _navigationService = navigationService;

        [ObservableProperty]
        public partial Product NewProduct { get; set; } = new();

        [RelayCommand]
        public async Task SaveProduct()
        {
            if (string.IsNullOrWhiteSpace(NewProduct.Name) || NewProduct.Price <= 0)
                return;

            _dbContext.Products.Add(NewProduct);
            await _dbContext.SaveChangesAsync();

            _navigationService.NavigateTo<ProductsViewModel>();
        }

        [RelayCommand]
        public void CancelProduct()
        {
            _navigationService.NavigateTo<ProductsViewModel>();
        }
    }
}
