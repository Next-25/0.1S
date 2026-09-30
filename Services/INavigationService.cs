using CommunityToolkit.Mvvm.ComponentModel;

namespace _01S.Services
{
    public interface INavigationService
    {
        ObservableObject? CurrentView { get; }
        void NavigateTo<TViewModel>()
            where TViewModel : ObservableObject;
        void NavigateTo<TViewModel, TParameter>(TParameter parameter)
            where TViewModel : ObservableObject;
    }
}
