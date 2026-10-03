using CommunityToolkit.Mvvm.ComponentModel;

namespace _01S.Services
{
    /// <summary>
    /// Создаёт и отображает модели представления, используя контейнер сервисов.
    /// </summary>
    /// <param name="serviceProvider">Поставщик зарегистрированных сервисов.</param>
    public partial class NavigationService(IServiceProvider serviceProvider)
        : ObservableObject,
            INavigationService
    {
        [ObservableProperty]
        public partial ObservableObject? CurrentView { get; set; }

        /// <summary>
        /// Создаёт и отображает модель представления без параметров.
        /// </summary>
        /// <typeparam name="TViewModel">Тип модели представления.</typeparam>
        public void NavigateTo<TViewModel>()
            where TViewModel : ObservableObject
        {
            CurrentView = serviceProvider.GetService(typeof(TViewModel)) as ObservableObject;
        }

        /// <summary>
        /// Создаёт модель представления, передаёт ей параметр и отображает её.
        /// </summary>
        /// <typeparam name="TViewModel">Тип модели представления.</typeparam>
        /// <typeparam name="TParameter">Тип параметра навигации.</typeparam>
        /// <param name="parameter">Параметр, передаваемый инициализируемой модели представления.</param>
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
