using CommunityToolkit.Mvvm.ComponentModel;

namespace _01S.Services
{
    public interface INavigationService
    {
        ObservableObject? CurrentView { get; }

        /// <summary>
        /// Создаёт и отображает модель представления без параметров.
        /// </summary>
        /// <typeparam name="TViewModel">Тип модели представления.</typeparam>
        void NavigateTo<TViewModel>()
            where TViewModel : ObservableObject;

        /// <summary>
        /// Создаёт модель представления, передаёт ей параметр и отображает её.
        /// </summary>
        /// <typeparam name="TViewModel">Тип модели представления.</typeparam>
        /// <typeparam name="TParameter">Тип параметра навигации.</typeparam>
        /// <param name="parameter">Параметр навигации.</param>
        void NavigateTo<TViewModel, TParameter>(TParameter parameter)
            where TViewModel : ObservableObject;
    }
}
