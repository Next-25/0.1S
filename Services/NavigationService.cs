using CommunityToolkit.Mvvm.ComponentModel;

namespace _01S.Services
{
    public partial class NavigationService(IServiceProvider serviceProvider)
        : ObservableObject,
            INavigationService
    {
        [ObservableProperty]
        public partial ObservableObject? CurrentView { get; set; }

        public void NavigateTo<TViewModel>()
            where TViewModel : ObservableObject
        {
            CurrentView = serviceProvider.GetService(typeof(TViewModel)) as ObservableObject;
        }

        public void NavigateTo<TViewModel, TParameter>(TParameter? parameter = default)
            where TViewModel : ObservableObject
        {
            var viewModel = serviceProvider.GetService(typeof(TViewModel)) as ObservableObject;

            // Если ViewModel реализует интерфейс для передаваемого параметра — вызываем Initialize
            if (viewModel is IInitializable<TParameter> initializable)
            {
                initializable.Initialize(parameter);
            }

            CurrentView = viewModel;
        }
    }
}
