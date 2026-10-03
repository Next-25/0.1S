namespace _01S.Services
{
    /// <summary>
    /// Интерфейс для ViewModel, принимающих параметр при навигации.
    /// </summary>
    /// <typeparam name="T">Тип передаваемого параметра (модель данных).</typeparam>
    interface IInitializable<T>
    {
        /// <summary>
        /// Инициализирует модель представления параметром навигации.
        /// </summary>
        /// <param name="parameter">Параметр навигации.</param>
        void Initialize(T? parameter);
    }
}
