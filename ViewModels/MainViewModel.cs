using _01S.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace _01S.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public INavigationService Navigation { get; }

        public MainViewModel(INavigationService navigation)
        {
            Navigation = navigation;
            // Стартовая страница
            Navigation.NavigateTo<HomeViewModel>();
        }

        [RelayCommand]
        private void NavigateHome() => Navigation.NavigateTo<HomeViewModel>();

        [RelayCommand]
        private void NavigateProducts() => Navigation.NavigateTo<ProductsViewModel>();

        [RelayCommand]
        private void NavigateDocuments() => Navigation.NavigateTo<DocumentsViewModel>();
    }
}
