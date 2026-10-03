using _01S.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace _01S.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public INavigationService Navigation { get; }

        /// <summary>
        /// Создаёт главную модель представления и открывает начальную страницу.
        /// </summary>
        /// <param name="navigation">Сервис навигации между страницами.</param>
        public MainViewModel(INavigationService navigation)
        {
            Navigation = navigation;
            // Стартовая страница
            Navigation.NavigateTo<HomeViewModel>();
        }

        /// <summary>
        /// Открывает главную страницу.
        /// </summary>
        [RelayCommand]
        private void NavigateHome() => Navigation.NavigateTo<HomeViewModel>();

        /// <summary>
        /// Открывает список товаров.
        /// </summary>
        [RelayCommand]
        private void NavigateProducts() => Navigation.NavigateTo<ProductsViewModel>();

        /// <summary>
        /// Открывает список документов.
        /// </summary>
        [RelayCommand]
        private void NavigateDocuments() => Navigation.NavigateTo<DocumentsViewModel>();
    }
}
